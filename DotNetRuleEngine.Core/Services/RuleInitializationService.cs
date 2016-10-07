using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Exceptions;
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

        public IList<IRule<T>> Initialize(IList<object> rules)
        {
            Initializer(rules);
            return rules.OfType<IRule<T>>().ToList();
        }

        private void Initializer(IList<object> rules, 
            IGeneralRule<T> nestingRule = null)
        {
            for (var i = 0; i < rules.Count; i++)
            {
                var rule = ResolveRule<IRule<T>>(rules[i]);

                rules[i] = rule;

                InitializeRule(rule, nestingRule);

                rule.Initialize();

                if (rule.IsNested) Initializer(rule.GetRules(), rule);
            }
        }

        public async Task<IList<IRuleAsync<T>>> InitializeAsync(IList<object> rules)
        {
            var initBag = new ConcurrentBag<Task>();
            InitializerAsync(rules, initBag);
            
            await Task.WhenAll(initBag);

            return rules.OfType<IRuleAsync<T>>().ToList();
        }

        private void InitializerAsync(IList<object> rules,
            ConcurrentBag<Task> initBag, IGeneralRule<T> nestingRule = null)
        {
            for (var i = 0; i < rules.Count; i++)
            {
                var rule = ResolveRule<IRuleAsync<T>>(rules[i]);

                rules[i] = rule;

                InitializeRule(rule, nestingRule);

                initBag.Add(rule.InitializeAsync());

                if (rule.IsNested) InitializerAsync(rule.GetRules(), initBag, rule);
            }
        }

        private void InitializeRule(IGeneralRule<T> rule, IGeneralRule<T> nestingRule = null)
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

        private TK ResolveRule<TK>(object ruleObject) where TK : class
        {
            var resolvedRule = default(TK);

            if (ruleObject is Type)
            {
                resolvedRule = _dependencyResolver.GetService((Type)ruleObject) as TK;

                if (resolvedRule == null) throw new UnsupportedRuleException();                
            }
            return (TK)(resolvedRule ?? ruleObject);
        }
    }
}