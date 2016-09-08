using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;

namespace DotNetRuleEngine.Core.Services
{
    internal sealed class RuleInitializationService<T> where T : class, new()
    {
        private readonly T _model;
        private readonly Guid _ruleEngineId;
        private readonly IDependencyResolver _dependencyResolver;

        public RuleInitializationService(T model, Guid ruleEngineId, IDependencyResolver dependencyResolver)
        {
            _model = model;
            _ruleEngineId = ruleEngineId;
            _dependencyResolver = dependencyResolver;
        }

        public void Initialize(IEnumerable<IGeneralRule<T>> rules, IGeneralRule<T> nestingRule = null)
        {
            foreach (var rule in rules.OfType<IRule<T>>())
            {
                RuleInitializer(rule, nestingRule);

                rule.Initialize();

                if (rule.IsNested) Initialize(rule.GetRules(), rule);
            }
        }

        public Task InitializeAsync(IEnumerable<IGeneralRule<T>> rules, IGeneralRule<T> nestingRule = null)
        {
            var initBag = new ConcurrentBag<Task>();
            Parallel.ForEach(rules.OfType<IRuleAsync<T>>(), rule =>
            {
                RuleInitializer(rule, nestingRule);

                initBag.Add(rule.InitializeAsync());

                if (rule.IsNested) InitializeAsync(rule.GetRules(), rule);
            });
            return Task.WhenAll(initBag);
        }

        private void RuleInitializer(IGeneralRule<T> rule, IGeneralRule<T> nestingRule = null)
        {
            rule.Model = _model;
            rule.Configuration = new RuleEngineConfiguration<T>(rule.Configuration) { RuleEngineId = _ruleEngineId };

            if (nestingRule != null && nestingRule.Configuration.NestedRulesInheritConstraint)
            {
                rule.Configuration.Constraint = nestingRule.Configuration.Constraint;
                rule.Configuration.NestedRulesInheritConstraint = true;
            }

            rule.Resolve = _dependencyResolver;
        }
    }
}