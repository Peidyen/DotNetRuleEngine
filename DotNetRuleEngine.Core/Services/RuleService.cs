using System.Collections.Generic;
using System.Linq;
using DotNetRuleEngine.Core.Extensions;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;

namespace DotNetRuleEngine.Core.Services
{
    internal class RuleService<T> where T : class, new()
    {
        private readonly T _model;
        private readonly IRuleEngineConfiguration<T> _ruleEngineConfiguration;
        private readonly ActiveRuleService<T> _activeRuleService;
        private readonly ICollection<IRuleResult> _ruleResults = new List<IRuleResult>();

        public RuleService(T model, IEnumerable<IGeneralRule<T>> rules,
            IRuleEngineConfiguration<T> ruleEngineConfiguration)
        {
            _model = model;
            _activeRuleService = new ActiveRuleService<T>(rules);
            _ruleEngineConfiguration = ruleEngineConfiguration;
        }

        public void Invoke(IEnumerable<IGeneralRule<T>> rules) => Execute(_activeRuleService.FilterActivatingRules(rules));

        private void Execute(IEnumerable<IGeneralRule<T>> rules)
        {
            foreach (var rule in OrderByExecutionOrder(rules))
            {
                InvokeNestedRules(rule.Configuration.InvokeNestedRulesFirst, rule);

                if (rule.CanInvoke(_model, _ruleEngineConfiguration.IsRuleEngineTerminated()))
                {
                    InvokePreactiveRules(rule);

                    TraceMessage.Verbose(rule, TraceMessage.BeforeInvoke);
                    rule.BeforeInvoke();

                    TraceMessage.Verbose(rule, TraceMessage.Invoke);
                    var ruleResult = rule.Invoke();

                    TraceMessage.Verbose(rule, TraceMessage.AfterInvoke);
                    rule.AfterInvoke();

                    AddToRuleResults(ruleResult, rule.GetType().Name);

                    rule.UpdateRuleEngineConfiguration(_ruleEngineConfiguration);

                    InvokeReactiveRules(rule);
                }

                InvokeNestedRules(!rule.Configuration.InvokeNestedRulesFirst, rule);
            }
        }

        public IRuleResult[] GetRuleResults() => _ruleResults.ToArray();

        private void InvokeReactiveRules(IRule<T> rule)
        {
            if (_activeRuleService.GetReactiveRules().ContainsKey(rule.GetType()))
            {
                Execute(_activeRuleService.GetReactiveRules()[rule.GetType()].OfType<IRule<T>>());
            }
        }

        private void InvokePreactiveRules(IRule<T> rule)
        {
            if (_activeRuleService.GetPreactiveRules().ContainsKey(rule.GetType()))
            {
                Execute(_activeRuleService.GetPreactiveRules()[rule.GetType()].OfType<IRule<T>>());
            }
        }

        private void AddToRuleResults(IRuleResult ruleResult, string ruleName)
        {
            ruleResult.AssignRuleName(ruleName);

            if (ruleResult != null) _ruleResults.Add(ruleResult);
        }

        private void InvokeNestedRules(bool invokeNestedRules, IGeneralRule<T> rule)
        {
            if (invokeNestedRules && rule.IsNested)
            {
                Execute(_activeRuleService.FilterActivatingRules(OrderByExecutionOrder(rule.GetRules())));
            }
        }

        private static IEnumerable<IRule<T>> OrderByExecutionOrder(IEnumerable<IGeneralRule<T>> rules)
        {
            rules = rules.ToList();
            return rules.GetRulesWithExecutionOrder().OfType<IRule<T>>()
                    .Concat(rules.GetRulesWithoutExecutionOrder().OfType<IRule<T>>());
        }
    }
}
