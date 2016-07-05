using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Extensions;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;

namespace DotNetRuleEngine.Core.Services
{
    internal class AsyncRuleService<T> where T : class, new()
    {
        private readonly T _model;
        private readonly IRuleEngineConfiguration<T> _ruleEngineConfiguration;
        private readonly ActiveRuleService<T> _activeRuleService;
        private ConcurrentBag<Task<IRuleResult>> ParallelRuleResults { get; } = new ConcurrentBag<Task<IRuleResult>>();

        public AsyncRuleService(T model,
            IEnumerable<IGeneralRule<T>> rules,
            IRuleEngineConfiguration<T> ruleEngineTerminated)
        {
            _model = model;
            _activeRuleService = new ActiveRuleService<T>(rules);
            _ruleEngineConfiguration = ruleEngineTerminated;
        }

        public ICollection<IRuleResult> AsyncRuleResults { get; } =  new List<IRuleResult>();

        public async Task InvokeAsyncRules(IEnumerable<IGeneralRule<T>> rules)
        {
            await ExecuteAsyncRules(_activeRuleService.FilterActivatingRules(rules));
        }

        private async Task ExecuteAsyncRules(IEnumerable<IGeneralRule<T>> rules)
        {
            rules = rules.ToList();

            await ExecuteParallelRules(rules);

            foreach (var asyncRule in OrderByExecutionOrder(rules))
            {
                await InvokeNestedRulesAsync(asyncRule.Configuration.InvokeNestedRulesFirst, asyncRule);

                if (asyncRule.CanInvoke(_model, _ruleEngineConfiguration.IsRuleEngineTerminated()))
                {
                    await InvokePreactiveRulesAsync(asyncRule);

                    TraceMessage.Verbose(asyncRule, TraceMessage.BeforeInvokeAsync);
                    await asyncRule.BeforeInvokeAsync();

                    TraceMessage.Verbose(asyncRule, TraceMessage.InvokeAsync);
                    var ruleResult = await asyncRule.InvokeAsync();

                    TraceMessage.Verbose(asyncRule, TraceMessage.AfterInvokeAsync);
                    await asyncRule.AfterInvokeAsync();

                    asyncRule.UpdateRuleEngineConfiguration(_ruleEngineConfiguration);

                    await InvokeReactiveRulesAsync(asyncRule);

                    ruleResult.AssignRuleName(asyncRule.GetType().Name);
                    AddToAsyncRuleResults(ruleResult);
                }

                await InvokeNestedRulesAsync(!asyncRule.Configuration.InvokeNestedRulesFirst, asyncRule);
            }
        }

        private async Task ExecuteParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            foreach (var pRule in GetParallelRules(rules))
            {
                await InvokeNestedRulesAsync(pRule.Configuration.InvokeNestedRulesFirst, pRule);

                if (pRule.CanInvoke(_model, _ruleEngineConfiguration.IsRuleEngineTerminated()))
                {
                    await InvokePreactiveRulesAsync(pRule);

                    ParallelRuleResults.Add(Task.Run(async () =>
                    {
                        TraceMessage.Verbose(pRule, TraceMessage.BeforeInvokeAsync);
                        await pRule.BeforeInvokeAsync();

                        TraceMessage.Verbose(pRule, TraceMessage.InvokeAsync);
                        var ruleResult = await pRule.InvokeAsync();

                        TraceMessage.Verbose(pRule, TraceMessage.AfterInvokeAsync);
                        await pRule.AfterInvokeAsync();

                        pRule.UpdateRuleEngineConfiguration(_ruleEngineConfiguration);

                        ruleResult.AssignRuleName(pRule.GetType().Name);

                        return ruleResult;
                    }));

                    await InvokeReactiveRulesAsync(pRule);
                }

                await InvokeNestedRulesAsync(!pRule.Configuration.InvokeNestedRulesFirst, pRule);
            }
        }

        private async Task InvokeReactiveRulesAsync(IRuleAsync<T> asyncRule)
        {
            if (_activeRuleService.GetReactiveRules().ContainsKey(asyncRule.GetType()))
            {
                await ExecuteAsyncRules(_activeRuleService.GetReactiveRules()[asyncRule.GetType()].OfType<IRuleAsync<T>>());
            }
        }

        private async Task InvokePreactiveRulesAsync(IRuleAsync<T> asyncRule)
        {
            if (_activeRuleService.GetPreactiveRules().ContainsKey(asyncRule.GetType()))
            {
                await ExecuteAsyncRules(_activeRuleService.GetPreactiveRules()[asyncRule.GetType()].OfType<IRuleAsync<T>>());
            }
        }

        public async Task<IRuleResult[]> GetAsyncRuleResults()
        {
            await Task.WhenAll(ParallelRuleResults);

            ParallelRuleResults.ToList().ForEach(rule =>
            {
                rule.Result.AssignRuleName(rule.GetType().Name);
                AddToAsyncRuleResults(rule.Result);
            });

            return AsyncRuleResults.ToArray();
        }

        private async Task InvokeNestedRulesAsync(bool invokeNestedRules, IGeneralRule<T> rule)
        {
            if (invokeNestedRules && rule.IsNested)
            {
                await ExecuteAsyncRules(_activeRuleService.FilterActivatingRules(rule.GetRules()));
            }
        }

        private void AddToAsyncRuleResults(IRuleResult ruleResult)
        {
            if (ruleResult != null) AsyncRuleResults.Add(ruleResult);
        }
        
        private static IEnumerable<IRuleAsync<T>> OrderByExecutionOrder(IEnumerable<IGeneralRule<T>> rules)
        {
            rules = rules.ToList();
            return rules.GetRulesWithExecutionOrder().OfType<IRuleAsync<T>>()
                    .Concat(rules.GetRulesWithoutExecutionOrder(rule => !((IRuleAsync<T>)rule).IsParallel).OfType<IRuleAsync<T>>());
        }

        private static IEnumerable<IRuleAsync<T>> GetParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            return rules.OfType<IRuleAsync<T>>()
                .Where(r => r.IsParallel && !r.Configuration.ExecutionOrder.HasValue)
                .OrderBy(r => r.GetType().Name)
                .ToList();
        }
    }
}
