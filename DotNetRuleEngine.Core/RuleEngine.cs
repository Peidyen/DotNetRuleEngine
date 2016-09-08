using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Exceptions;
using DotNetRuleEngine.Core.Extensions;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;
using DotNetRuleEngine.Core.Services;

namespace DotNetRuleEngine.Core
{
    /// <summary>
    /// Rule Engine.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class RuleEngine<T> where T : class, new()
    {
        private T _model;
        private IDependencyResolver _dependencyResolver;
        private RuleService<T> _ruleService;
        private AsyncRuleService<T> _asyncRuleService;
        
        private readonly Guid _ruleEngineId = Guid.NewGuid();
        private readonly RuleEngineConfiguration<T> _ruleEngineConfiguration = new RuleEngineConfiguration<T>(new Configuration<T>());
        private readonly List<IGeneralRule<T>> _rules = new List<IGeneralRule<T>>();
        private readonly List<Type> _rulesAsType = new List<Type>();

        /// <summary>
        /// Rule engine ctor.
        /// </summary>
        private RuleEngine() { }

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
            new RuleEngine<T> {_model = instance, _dependencyResolver = dependencyResolver};

        /// <summary>
        /// Used to add rules to nestingRule engine.
        /// </summary>
        /// <param name="rules">Rule(s) list.</param>
        public void AddRules(params IGeneralRule<T>[] rules) => _rules.AddRange(rules);

        /// <summary>
        /// Used to add rules to nestingRule engine.
        /// </summary>
        /// <param name="rules">Rule(s) list.</param>
        public void AddRules(params Type[] rules) => _rulesAsType.AddRange(rules);

        /// <summary>
        /// Used to set instance.
        /// </summary>
        /// <param name="instance">_model</param>
        public void SetInstance(T instance) => _model = instance;

        /// <summary>
        /// Used to execute async rules.
        /// </summary>
        /// <returns></returns>
        public async Task<IRuleResult[]> ExecuteAsync()
        {
            var rules = GetRules();

            if (!rules.Any()) return await _asyncRuleService.GetAsyncRuleResultsAsync();

            await new RuleInitializationService<T>(_model, _ruleEngineId, _dependencyResolver).InitializeAsync(rules);

            _asyncRuleService = new AsyncRuleService<T>(_model, rules, _ruleEngineConfiguration);

            await _asyncRuleService.InvokeAsyncRules(rules);

            return await _asyncRuleService.GetAsyncRuleResultsAsync();
        }

        /// <summary>
        /// Used to execute rules.
        /// </summary>
        /// <returns></returns>
        public IRuleResult[] Execute()
        {
            var rules = GetRules();

            if (!rules.Any()) return _ruleService.GetRuleResults();

            new RuleInitializationService<T>(_model, _ruleEngineId, _dependencyResolver).Initialize(rules);

            _ruleService = new RuleService<T>(_model, rules, _ruleEngineConfiguration);

            _ruleService.Invoke(rules);

            return _ruleService.GetRuleResults();
        }

        private List<IGeneralRule<T>> GetRules()
        {
            _model.Validate();
            var rules = (_rules.Any() ? _rules : ResolveRules(_rulesAsType)).ToList();

            if (rules.Any(r => r == null)) throw new UnsupportedRuleException();

            return rules;
        }

        private IEnumerable<IGeneralRule<T>> ResolveRules(IEnumerable<Type> types)
        {
            if (_dependencyResolver == null) throw new DependencyResolverNotFoundException();

            return types.Select(t => _dependencyResolver.GetService(t) as IGeneralRule<T>);
        }
    }
}