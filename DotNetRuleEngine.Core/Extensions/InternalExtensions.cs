using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using DotNetRuleEngine.Core.Exceptions;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core.Extensions
{
    internal static class InternalExtensions
    {
        public static bool CanInvoke<T>(this IGeneralRule<T> rule, T model, bool terminated) where T : class, new() => 
            !rule.Configuration.Skip && rule.Configuration.Constraint.Invoke(model) && !terminated;

        public static bool Invoke<T>(this Expression<Predicate<T>> predicate, T model) =>
            predicate == null || predicate.Compile().Invoke(model);


        public static void AssignRuleName(this IRuleResult ruleResult, string ruleName)
        {
            if (ruleResult != null) ruleResult.Name = ruleResult.Name ?? ruleName;
        }

        public static void Validate<T>(this T model)
        {
            if (model == null) throw new ModelInstanceNotFoundException();
        }

        public static void UpdateRuleEngineConfiguration<T>(this IGeneralRule<T> rule,
            IConfiguration<T> ruleEngineConfiguration) where T : class, new()
        {
            if (ruleEngineConfiguration.Terminate == null && rule.Configuration.Terminate == true)
            {
                ruleEngineConfiguration.Terminate = true;
            }
        }

        public static bool IsRuleEngineTerminated<T>(this IConfiguration<T> ruleEngineConfiguration) where T : class, new()
            => ruleEngineConfiguration.Terminate != null && ruleEngineConfiguration.Terminate.Value;

        public static ICollection<IGeneralRule<T>> GetRulesWithExecutionOrder<T>(this IEnumerable<IGeneralRule<T>> rules,
            Func<IGeneralRule<T>, bool> condition = null) where T : class, new()
        {
            condition = condition ?? (rule => true);

            return rules.Where(r => r.Configuration.ExecutionOrder.HasValue)
                .Where(condition)
                .OrderBy(r => r.Configuration.ExecutionOrder)
                .ToList();
        }

        public static ICollection<IGeneralRule<T>> GetRulesWithoutExecutionOrder<T>(this IEnumerable<IGeneralRule<T>> rules,
            Func<IGeneralRule<T>, bool> condition = null) where T : class, new()
        {
            condition = condition ?? (k => true);

            return rules.Where(r => !r.Configuration.ExecutionOrder.HasValue)
                .Where(condition)
                .ToList();
        }

        public static IEnumerable<IGeneralRule<T>> GetResolvedRules<T>(this IGeneralRule<T> rule) where T : class, new()
        {
            return rule.GetRules().OfType<IGeneralRule<T>>();
        }

    }
}