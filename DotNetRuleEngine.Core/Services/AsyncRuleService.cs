﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Extensions;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;

namespace DotNetRuleEngine.Core.Services
{
    internal sealed class AsyncRuleService<T> where T : class, new()
    {
        private readonly T _model;
        private readonly IList<IRuleAsync<T>> _rules;
        private readonly IRuleEngineConfiguration<T> _ruleEngineConfiguration;
        private readonly RxRuleService<IRuleAsync<T>, T> _rxRuleService;
        private readonly ConcurrentBag<IRuleResult> _asyncRuleResults = new ConcurrentBag<IRuleResult>();
        private readonly ConcurrentBag<Task<IRuleResult>> _parallelRuleResults = new ConcurrentBag<Task<IRuleResult>>();

        public AsyncRuleService(T model, IList<IRuleAsync<T>> rules,
            IRuleEngineConfiguration<T> ruleEngineTerminated)
        {
            _model = model;
            _rules = rules;
            _rxRuleService = new RxRuleService<IRuleAsync<T>, T>(_rules);
            _ruleEngineConfiguration = ruleEngineTerminated;
        }

        public async Task InvokeAsyncRules()
        {
            await ExecuteRulesAsync(_rxRuleService.FilterRxRules(_rules));
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

        private async Task ExecuteRulesAsync(IList<IRuleAsync<T>> rules)
        {
            await ExecuteParallelRules(rules);

            foreach (var rule in OrderByExecutionOrder(rules))
            {
                await InvokeNestedRulesAsync(rule.Configuration.InvokeNestedRulesFirst, rule);

                if (rule.CanInvoke(_model, _ruleEngineConfiguration.IsRuleEngineTerminated()))
                {
                    try
                    {
                        await InvokeProactiveRulesAsync(rule);

                        AddToAsyncRuleResults(await ExecuteRuleAsync(rule));
                        rule.UpdateRuleEngineConfiguration(_ruleEngineConfiguration);

                        await InvokeReactiveRulesAsync(rule);
                    }
                    catch (Exception exception)
                    {
                        rule.UnhandledException = exception;

                        if (_rxRuleService.GetExceptionRules().ContainsKey(rule.GetType()))
                        {
                            await InvokeExceptionRulesAsync(rule);
                        }
                        else
                        {
                            var globalExceptionHandler = _rules.GetGlobalExceptionHandler();

                            if (globalExceptionHandler is IRuleAsync<T>)
                            {
                                globalExceptionHandler.UnhandledException = exception;
                                await ExecuteRulesAsync(new List<IRuleAsync<T>> { (IRuleAsync<T>)globalExceptionHandler });
                            }

                            throw;
                        }
                    }

                }

                await InvokeNestedRulesAsync(!rule.Configuration.InvokeNestedRulesFirst, rule);
            }
        }

        private async Task ExecuteParallelRules(IList<IRuleAsync<T>> rules)
        {
            foreach (var rule in GetParallelRules(rules))
            {
                await InvokeNestedRulesAsync(rule.Configuration.InvokeNestedRulesFirst, rule);

                if (rule.CanInvoke(_model, _ruleEngineConfiguration.IsRuleEngineTerminated()))
                {
                    await InvokeProactiveRulesAsync(rule);

                    _parallelRuleResults.Add(await Task.Factory.StartNew(async () =>
                    {
                        IRuleResult ruleResult = null;

                        try
                        {
                            ruleResult = await ExecuteRuleAsync(rule);
                        }
                        catch (Exception exception)
                        {
                            rule.UnhandledException = exception;

                            if (_rxRuleService.GetExceptionRules().ContainsKey(rule.GetType()))
                            {
                                await InvokeExceptionRulesAsync(rule);
                            }
                            else
                            {
                                throw;
                            }
                        }

                        return ruleResult;

                    }, rule.ParellelConfiguration.CancellationTokenSource?.Token ?? CancellationToken.None,
                        rule.ParellelConfiguration.TaskCreationOptions,
                        rule.ParellelConfiguration.TaskScheduler));

                    await InvokeReactiveRulesAsync(rule);
                }

                await InvokeNestedRulesAsync(!rule.Configuration.InvokeNestedRulesFirst, rule);
            }
        }

        private async Task<IRuleResult> ExecuteRuleAsync(IRuleAsync<T> rule)
        {
            TraceMessage.Verbose(rule, TraceMessage.BeforeInvokeAsync);
            await rule.BeforeInvokeAsync();

            if (rule.IsParallel && rule.ParellelConfiguration.CancellationTokenSource != null &&
                rule.ParellelConfiguration.CancellationTokenSource.Token.IsCancellationRequested)
            {
                return null;
            }

            TraceMessage.Verbose(rule, TraceMessage.InvokeAsync);
            var ruleResult = await rule.InvokeAsync();

            TraceMessage.Verbose(rule, TraceMessage.AfterInvokeAsync);
            await rule.AfterInvokeAsync();

            ruleResult.AssignRuleName(rule.GetType().Name);

            return ruleResult;
        }

        private async Task InvokeReactiveRulesAsync(IRuleAsync<T> asyncRule)
        {
            if (_rxRuleService.GetReactiveRules().ContainsKey(asyncRule.GetType()))
            {
                await ExecuteRulesAsync(_rxRuleService.GetReactiveRules()[asyncRule.GetType()]);
            }
        }

        private async Task InvokeProactiveRulesAsync(IRuleAsync<T> asyncRule)
        {
            if (_rxRuleService.GetProactiveRules().ContainsKey(asyncRule.GetType()))
            {
                await ExecuteRulesAsync(_rxRuleService.GetProactiveRules()[asyncRule.GetType()]);
            }
        }

        private async Task InvokeExceptionRulesAsync(IRuleAsync<T> asyncRule)
        {
            var exceptionRules =
                _rxRuleService.GetExceptionRules()[asyncRule.GetType()]
                    .Select(rule =>
                    {
                        rule.UnhandledException = asyncRule.UnhandledException;
                        return rule;
                    }).ToList();

            await ExecuteRulesAsync(exceptionRules);
        }

        private async Task InvokeNestedRulesAsync(bool invokeNestedRules, IRuleAsync<T> rule)
        {
            if (invokeNestedRules && rule.IsNested)
            {
                await ExecuteRulesAsync(_rxRuleService.FilterRxRules(rule.GetRules().OfType<IRuleAsync<T>>().ToList()));
            }
        }

        private void AddToAsyncRuleResults(IRuleResult ruleResult)
        {
            if (ruleResult != null) _asyncRuleResults.Add(ruleResult);
        }

        private static IEnumerable<IRuleAsync<T>> OrderByExecutionOrder(IList<IRuleAsync<T>> rules)
        {
            return rules.GetRulesWithExecutionOrder().OfType<IRuleAsync<T>>()
                    .Concat(rules.GetRulesWithoutExecutionOrder(rule => !((IRuleAsync<T>)rule).IsParallel).OfType<IRuleAsync<T>>());
        }

        private static IList<IRuleAsync<T>> GetParallelRules(IList<IRuleAsync<T>> rules)
        {
            return rules.Where(r => r.IsParallel && !r.Configuration.ExecutionOrder.HasValue)
                .AsParallel()
                .ToList();
        }
    }
}
