using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Exceptions;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core
{
    /// <summary>
    /// Rule Engine.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class RuleEngine<T> where T : class, new()
    {
        private T _instance;
        private IDependencyResolver _dependencyResolver;
        private ConcurrentDictionary<Type, IList<IGeneralRule<T>>> _reactiveRules;
        private ConcurrentDictionary<Type, IList<IGeneralRule<T>>> _preactiveRules;

        private readonly Guid _ruleEngineId = Guid.NewGuid();
        private readonly RuleEngineConfiguration<T> _ruleEngineConfiguration = new RuleEngineConfiguration<T>(new Configuration<T>());
        private readonly List<IGeneralRule<T>> _rules = new List<IGeneralRule<T>>();
        private readonly ICollection<IRuleResult> _ruleResults = new List<IRuleResult>();
        private readonly ICollection<IRuleResult> _asyncRuleResults = new List<IRuleResult>();
        private readonly ConcurrentBag<Task<IRuleResult>> _parallelRuleResults = new ConcurrentBag<Task<IRuleResult>>();

        /// <summary>
        /// Rule engine ctor.
        /// </summary>
        private RuleEngine()
        {
        }

        /// <summary>
        /// Set dependency resolver
        /// </summary>
        /// <param name="dependencyResolver"></param>
        public void SetDependencyResolver(IDependencyResolver dependencyResolver) => _dependencyResolver = dependencyResolver;

        /// <summary>
        /// Get a new instance of RuleEngine
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="dependencyResolver"></param>
        /// <returns></returns>
        public static RuleEngine<T> GetInstance(T instance = null, IDependencyResolver dependencyResolver = null) =>
            new RuleEngine<T> { _instance = instance, _dependencyResolver = dependencyResolver };

        /// <summary>
        /// Used to add rules to nestingRule engine.
        /// </summary>
        /// <param name="rules">Rule(s) list.</param>
        public void AddRules(params IGeneralRule<T>[] rules) => _rules.AddRange(rules);

        /// <summary>
        /// Used to set instance.
        /// </summary>
        /// <param name="instance">_instance</param>
        public void SetInstance(T instance) => _instance = instance;

        /// <summary>
        /// Used to execute async rules.
        /// </summary>
        /// <returns></returns>
        public async Task<IRuleResult[]> ExecuteAsync()
        {
            ValidateInstance();

            if (!_rules.Any()) return _asyncRuleResults.ToArray();

            await InitializeAsync(_rules);

            _reactiveRules = GetReactiveRules();
            _preactiveRules = GetPreactiveRules();

            await ExecuteAsyncRules(FilterActivatingRules(_rules));

            await Task.WhenAll(_parallelRuleResults);

            _parallelRuleResults.ToList().ForEach(rule =>
            {
                AddToAsyncRuleResults(rule.Result, rule.Result?.Name);
            });

            return _asyncRuleResults.ToArray();
        }

        /// <summary>
        /// Used to execute rules.
        /// </summary>
        /// <returns></returns>
        public IRuleResult[] Execute()
        {
            ValidateInstance();

            if (_rules == null || !_rules.Any()) return _ruleResults.ToArray();

            Initialize(_rules);

            _reactiveRules = GetReactiveRules();
            _preactiveRules = GetPreactiveRules();

            Execute(FilterActivatingRules(_rules));

            return _ruleResults.ToArray();
        }

        private void Execute(IEnumerable<IGeneralRule<T>> rules)
        {
            foreach (var rule in OrderByExecutionOrder(rules))
            {
                InvokeNestedRules(rule.Configuration.InvokeNestedRulesFirst, rule);

                if (CanInvoke(rule.Configuration))
                {
                    rule.Model = _instance;

                    if (_preactiveRules.ContainsKey(rule.GetType())) Execute(_preactiveRules[rule.GetType()]);

                    TraceMessage.Verbose(rule, TraceMessage.BeforeInvoke);
                    rule.BeforeInvoke();

                    TraceMessage.Verbose(rule, TraceMessage.Invoke);
                    var ruleResult = rule.Invoke();

                    TraceMessage.Verbose(rule, TraceMessage.AfterInvoke);
                    rule.AfterInvoke();

                    AddToRuleResults(ruleResult, rule.GetType().Name);

                    UpdateRuleEngineConfiguration(rule.Configuration);

                    if (_reactiveRules.ContainsKey(rule.GetType())) Execute(_reactiveRules[rule.GetType()]);
                }

                InvokeNestedRules(!rule.Configuration.InvokeNestedRulesFirst, rule);
            }
        }

        private async Task ExecuteAsyncRules(IEnumerable<IGeneralRule<T>> rules)
        {
            rules = rules.ToList();
            await ExecuteParallelRules(rules);

            foreach (var asyncRule in OrderByAsyncRuleExecutionOrder(rules))
            {
                await InvokeNestedRulesAsync(asyncRule.Configuration.InvokeNestedRulesFirst, asyncRule);

                if (CanInvoke(asyncRule.Configuration))
                {
                    asyncRule.Model = _instance;

                    if (_preactiveRules.ContainsKey(asyncRule.GetType())) await ExecuteAsyncRules(_preactiveRules[asyncRule.GetType()]);

                    TraceMessage.Verbose(asyncRule, TraceMessage.BeforeInvokeAsync);
                    await asyncRule.BeforeInvokeAsync();

                    TraceMessage.Verbose(asyncRule, TraceMessage.InvokeAsync);
                    var ruleResult = await asyncRule.InvokeAsync();

                    TraceMessage.Verbose(asyncRule, TraceMessage.AfterInvokeAsync);
                    await asyncRule.AfterInvokeAsync();

                    UpdateRuleEngineConfiguration(asyncRule.Configuration);

                    if (_reactiveRules.ContainsKey(asyncRule.GetType())) await ExecuteAsyncRules(_reactiveRules[asyncRule.GetType()]);

                    AddToAsyncRuleResults(ruleResult, asyncRule.GetType().Name);
                }

                await InvokeNestedRulesAsync(!asyncRule.Configuration.InvokeNestedRulesFirst, asyncRule);
            }
        }

        private async Task ExecuteParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            foreach (var pRule in GetParallelRules(rules))
            {
                await InvokeNestedRulesAsync(pRule.Configuration.InvokeNestedRulesFirst, pRule);

                if (CanInvoke(pRule.Configuration))
                {
                    pRule.Model = _instance;

                    if (_preactiveRules.ContainsKey(pRule.GetType())) await ExecuteAsyncRules(_preactiveRules[pRule.GetType()]);

                    _parallelRuleResults.Add(Task.Run(async () =>
                    {
                        TraceMessage.Verbose(pRule, TraceMessage.BeforeInvokeAsync);
                        await pRule.BeforeInvokeAsync();

                        TraceMessage.Verbose(pRule, TraceMessage.InvokeAsync);
                        var ruleResult = await pRule.InvokeAsync();

                        TraceMessage.Verbose(pRule, TraceMessage.AfterInvokeAsync);
                        await pRule.AfterInvokeAsync();

                        UpdateRuleEngineConfiguration(pRule.Configuration);

                        AssignRuleName(ruleResult, pRule.GetType().Name);

                        return ruleResult;
                    }));

                    if (_reactiveRules.ContainsKey(pRule.GetType())) await ExecuteAsyncRules(_reactiveRules[pRule.GetType()]);
                }

                await InvokeNestedRulesAsync(!pRule.Configuration.InvokeNestedRulesFirst, pRule);
            }
        }

        private ConcurrentDictionary<Type, IList<IGeneralRule<T>>> GetReactiveRules()
        {
            var reactiveRules = new ConcurrentDictionary<Type, IList<IGeneralRule<T>>>();
            GetActivatingRules(_rules, reactiveRules, rule => rule.IsReactive);

            return reactiveRules;
        }

        private ConcurrentDictionary<Type, IList<IGeneralRule<T>>> GetPreactiveRules()
        {
            var preactiveRules = new ConcurrentDictionary<Type, IList<IGeneralRule<T>>>();
            GetActivatingRules(_rules, preactiveRules, rule => rule.IsPreactive);

            return preactiveRules;
        }

        private static void GetActivatingRules(IEnumerable<IGeneralRule<T>> rules,
            ConcurrentDictionary<Type, IList<IGeneralRule<T>>> activatingRules, Predicate<IGeneralRule<T>> predicate)
        {
            Parallel.ForEach(rules, r =>
            {
                if (predicate(r))
                { 
                    activatingRules.AddOrUpdate(r.ObserveRule, new[] { r }, (type, list) =>
                    {
                        list.Add(r);
                        return list;
                    });
                }
                if (r.IsNested) GetActivatingRules(r.GetRules(), activatingRules, predicate);
            });
        }

        private static IEnumerable<IGeneralRule<T>> FilterActivatingRules(IEnumerable<IGeneralRule<T>> rules) =>
            rules.Where(r => !r.IsReactive && !r.IsPreactive).AsParallel().ToList();

        private async Task InvokeNestedRulesAsync(bool invokeNestedRules, IGeneralRule<T> rule)
        {
            if (invokeNestedRules && rule.IsNested) await ExecuteAsyncRules(FilterActivatingRules(rule.GetRules()));
        }

        private void InvokeNestedRules(bool invokeNestedRules, IGeneralRule<T> rule)
        {
            if (invokeNestedRules && rule.IsNested) Execute(FilterActivatingRules(OrderByExecutionOrder(rule.GetRules())));
        }

        private void ValidateInstance()
        {
            if (_instance == null) throw new ModelInstanceNotFoundException();
        }

        private bool Constrained(Expression<Predicate<T>> predicate) => predicate == null || predicate.Compile().Invoke(_instance);

        private void Initialize(IEnumerable<IGeneralRule<T>> rules, IGeneralRule<T> nestingRule = null)
        {
            foreach (var r in rules.OfType<IRule<T>>())
            {
                Initialize(r, nestingRule);

                r.Initialize();

                if (r.IsNested) Initialize(r.GetRules(), r);
            }
        }

        private async Task InitializeAsync(IEnumerable<IGeneralRule<T>> rules, IGeneralRule<T> nestingRule = null)
        {
            foreach (var r in rules.OfType<IRuleAsync<T>>())
            {
                Initialize(r, nestingRule);

                await r.InitializeAsync();

                if (r.IsNested) await InitializeAsync(r.GetRules(), r);
            }
        }

        private void Initialize(IGeneralRule<T> rule, IGeneralRule<T> nestingRule)
        {
            rule.Model = _instance;
            rule.Configuration = new RuleEngineConfiguration<T>(rule.Configuration) { RuleEngineId = _ruleEngineId };

            if (nestingRule != null && nestingRule.Configuration.NestedRulesInheritConstraint)
            {
                rule.Configuration.Constraint = nestingRule.Configuration.Constraint;
                rule.Configuration.NestedRulesInheritConstraint = true;
            }

            rule.DependencyResolver = _dependencyResolver ?? new NullDependencyResolver();
        }

        private void AddToRuleResults(IRuleResult ruleResult, string ruleName)
        {
            if (ruleResult != null) _ruleResults.Add(AssignRuleName(ruleResult, ruleName));
        }

        private void AddToAsyncRuleResults(IRuleResult ruleResult, string ruleName)
        {
            if (ruleResult != null) _asyncRuleResults.Add(AssignRuleName(ruleResult, ruleName));
        }

        private void UpdateRuleEngineConfiguration(IConfiguration<T> ruleConfiguration)
        {
            if (_ruleEngineConfiguration.Terminate == null && ruleConfiguration.Terminate == true)
            {
                _ruleEngineConfiguration.Terminate = true;
            }
        }

        private bool CanInvoke(IConfiguration<T> configuration) => !configuration.Skip && 
            Constrained(configuration.Constraint) && !RuleEngineTerminated();

        private bool RuleEngineTerminated() => _ruleEngineConfiguration.Terminate != null && _ruleEngineConfiguration.Terminate.Value;

        private static IEnumerable<IRuleAsync<T>> OrderByAsyncRuleExecutionOrder(IEnumerable<IGeneralRule<T>> rules)
        {
            var generalRules = rules.ToList();

            var rulesWithExecutionOrder = GetRulesWithExecutionOrder<IRuleAsync<T>>(generalRules, 
                r => r.Configuration.ExecutionOrder.HasValue);

            var rulesWithoutExecutionOrder = GetRulesWithoutExecutionOrder<IRuleAsync<T>>(generalRules, 
                r => !r.IsParallel && !r.Configuration.ExecutionOrder.HasValue);

            return rulesWithExecutionOrder.Concat(rulesWithoutExecutionOrder);
        }

        private static IEnumerable<IRule<T>> OrderByExecutionOrder(IEnumerable<IGeneralRule<T>> rules)
        {
            var generalRules = rules.ToList();
            var rulesWithExecutionOrder = GetRulesWithExecutionOrder<IRule<T>>(generalRules);
            var rulesWithoutExecutionOrder = GetRulesWithoutExecutionOrder<IRule<T>>(generalRules);

            return rulesWithExecutionOrder.Concat(rulesWithoutExecutionOrder).ToList();
        }

        private static IRuleResult AssignRuleName(IRuleResult ruleResult, string ruleName)
        {
            if (ruleResult != null) ruleResult.Name = ruleResult.Name ?? ruleName;

            return ruleResult;
        }

        private static ICollection<TK> GetRulesWithoutExecutionOrder<TK>(IEnumerable<IGeneralRule<T>> rules,
            Func<TK, bool> condition = null) where TK : IGeneralRule<T>
        {
            condition = condition ?? (k => true);

            return rules.OfType<TK>()
                        .Where(r => !r.Configuration.ExecutionOrder.HasValue)
                        .Where(condition)
                        .AsParallel()
                        .ToList();
        }

        private static ICollection<TK> GetRulesWithExecutionOrder<TK>(IEnumerable<IGeneralRule<T>> rules,
            Func<TK, bool> condition = null) where TK : IGeneralRule<T>
        {
            condition = condition ?? (k => true);

            return rules.OfType<TK>()
                .Where(r => r.Configuration.ExecutionOrder.HasValue)
                .Where(condition)
                .OrderBy(r => r.Configuration.ExecutionOrder)
                .AsParallel()
                .ToList();
        }

        private static IEnumerable<IRuleAsync<T>> GetParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            return rules.OfType<IRuleAsync<T>>()
                        .Where(r => r.IsParallel && !r.Configuration.ExecutionOrder.HasValue)
                        .OrderBy(r => r.GetType().Name)
                        .AsParallel()
                        .ToList();
        }
    }
}