using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Extensions;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;

namespace DotNetRuleEngine.Core.Services
{
    internal sealed class AsyncRuleService<T> where T : class, new()
    {
        private readonly T _model;
        private readonly IRuleEngineConfiguration<T> _ruleEngineConfiguration;
        private readonly ActiveRuleService<T> _activeRuleService;
        private readonly ConcurrentBag<IRuleResult> _asyncRuleResults = new ConcurrentBag<IRuleResult>();
        private readonly ConcurrentBag<Task<IRuleResult>> _parallelRuleResults = new ConcurrentBag<Task<IRuleResult>>();

        public AsyncRuleService(T model,
            IEnumerable<IGeneralRule<T>> rules,
            IRuleEngineConfiguration<T> ruleEngineTerminated)
        {
            _model = model;
            _activeRuleService = new ActiveRuleService<T>(rules);
            _ruleEngineConfiguration = ruleEngineTerminated;
        }

        public async Task InvokeAsyncRules(IEnumerable<IGeneralRule<T>> rules)
        {
            await ExecuteAsyncRules(_activeRuleService.FilterActivatingRules(rules));
        }

        private async Task ExecuteAsyncRules(IEnumerable<IGeneralRule<T>> rules)
        {
            rules = rules.ToList();

            await ExecuteParallelRules(rules);

            foreach (var rule in OrderByExecutionOrder(rules))
            {
                await InvokeNestedRulesAsync(rule.Configuration.InvokeNestedRulesFirst, rule);

                if (rule.CanInvoke(_model, _ruleEngineConfiguration.IsRuleEngineTerminated()))
                {
                    try
                    {
                        await InvokePreactiveRulesAsync(rule);

                        AddToAsyncRuleResults(await ExecuteRule(rule));
                        rule.UpdateRuleEngineConfiguration(_ruleEngineConfiguration);

                        await InvokeReactiveRulesAsync(rule);
                    }
                    catch (Exception exception)
                    {
                        rule.UnhandledException = exception;

                        if (_activeRuleService.GetExceptionRules().ContainsKey(rule.GetType()))
                        {
                            await InvokeExceptionRulesAsync(rule);
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                await InvokeNestedRulesAsync(!rule.Configuration.InvokeNestedRulesFirst, rule);
            }
        }

        private async Task ExecuteParallelRules(IEnumerable<IGeneralRule<T>> rules)
        {
            foreach (var rule in GetParallelRules(rules))
            {
                await InvokeNestedRulesAsync(rule.Configuration.InvokeNestedRulesFirst, rule);

                if (rule.CanInvoke(_model, _ruleEngineConfiguration.IsRuleEngineTerminated()))
                {
                    await InvokePreactiveRulesAsync(rule);

                    _parallelRuleResults.Add(await Task.Factory.StartNew(async () =>
                    {
                        IRuleResult ruleResult = null;

                        try
                        {
                            ruleResult = await ExecuteRule(rule);
                        }
                        catch (Exception exception)
                        {
                            rule.UnhandledException = exception;

                            if (_activeRuleService.GetExceptionRules().ContainsKey(rule.GetType()))
                            {
                                await InvokeExceptionRulesAsync(rule);
                            }
                            else
                            {
                                throw;
                            }
                        }

                        return ruleResult;
                    }, TaskCreationOptions.PreferFairness));

                    await InvokeReactiveRulesAsync(rule);
                }

                await InvokeNestedRulesAsync(!rule.Configuration.InvokeNestedRulesFirst, rule);
            }
        }

        private static async Task<IRuleResult> ExecuteRule(IRuleAsync<T> rule)
        {
            TraceMessage.Verbose(rule, TraceMessage.BeforeInvokeAsync);
            await rule.BeforeInvokeAsync();

            TraceMessage.Verbose(rule, TraceMessage.InvokeAsync);
            var ruleResult = await rule.InvokeAsync();

            TraceMessage.Verbose(rule, TraceMessage.AfterInvokeAsync);
            await rule.AfterInvokeAsync();

            ruleResult.AssignRuleName(rule.GetType().Name);

            return ruleResult;
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

        private async Task InvokeExceptionRulesAsync(IGeneralRule<T> asyncRule)
        {
            var exceptionRules = _activeRuleService.GetExceptionRules()[asyncRule.GetType()].OfType<IRuleAsync<T>>().ToList();

            exceptionRules.ForEach(exceptionRule => exceptionRule.UnhandledException = asyncRule.UnhandledException);

            await ExecuteAsyncRules(exceptionRules);
        }

        public async Task<IRuleResult[]> GetAsyncRuleResultsAsync()
        {
            await Task.WhenAll(_parallelRuleResults);

            Parallel.ForEach(_parallelRuleResults, rule =>
            {
                rule.Result.AssignRuleName(rule.GetType().Name);
                AddToAsyncRuleResults(rule.Result);
            });

            return _asyncRuleResults.ToArray();
        }

        private async Task InvokeNestedRulesAsync(bool invokeNestedRules, IGeneralRule<T> rule)
        {
            if (invokeNestedRules && rule.IsNested)
            {
                await ExecuteAsyncRules(_activeRuleService.FilterActivatingRules(rule.GetResolvedRules()));
            }
        }

        private void AddToAsyncRuleResults(IRuleResult ruleResult)
        {
            if (ruleResult != null) _asyncRuleResults.Add(ruleResult);
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
                .AsParallel();
        }
    }
}
