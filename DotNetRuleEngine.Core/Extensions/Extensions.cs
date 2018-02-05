using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;

namespace DotNetRuleEngine.Core.Extensions
{
    public static class Extensions
    {
        public static T To<T>(this object @object) => (T) @object;

        public static T To<T>(this Task<object> @object) => (T) @object?.Result;

        public static Guid GetRuleEngineId<T>(this IGeneralRule<T> rule) where T : class, new() =>
            rule.Configuration.To<RuleEngineConfiguration<T>>().RuleEngineId;

        public static string GetRuleName<T>(this IGeneralRule<T> rule) where T : class, new() =>
            rule.GetType().Name;

        public static IRuleResult FindRuleResult<T>(this IEnumerable<IRuleResult> ruleResults) =>
            ruleResults.FirstOrDefault(r => string.Equals(r.Name, typeof(T).Name, StringComparison.InvariantCultureIgnoreCase));

        public static IEnumerable<IRuleResult> FindRuleResults<T>(this IEnumerable<IRuleResult> ruleResults) =>
            ruleResults.Where(r => string.Equals(r.Name, typeof(T).Name, StringComparison.InvariantCultureIgnoreCase));

        public static IRuleResult FindRuleResult(this IEnumerable<IRuleResult> ruleResults, string ruleName) =>
            ruleResults.FirstOrDefault(r => string.Equals(r.Name, ruleName, StringComparison.InvariantCultureIgnoreCase));

        public static IEnumerable<IRuleResult> FindRuleResults(this IEnumerable<IRuleResult> ruleResults, string ruleName) =>
            ruleResults.Where(r => string.Equals(r.Name, ruleName, StringComparison.InvariantCultureIgnoreCase));

        public static RuleEngine<T> ApplyRules<T>(this RuleEngine<T> ruleEngineExecutor,
            params object[] rules) where T : class, new()
        {
            ruleEngineExecutor.AddRules(rules);

            return ruleEngineExecutor;
        }

        public static IEnumerable<IRuleResult> GetErrors(this IEnumerable<IRuleResult> ruleResults)
            => ruleResults.Where(r => r.Error != null);

        public static bool AnyError(this IEnumerable<IRuleResult> ruleResults) => ruleResults.Any(r => r.Error != null);

        public static RuleType GetRuleType<T>(this IGeneralRule<T> rule) where T : class, new()
        {
            if (rule.IsProactive) return RuleType.ProActiveRule;
            if (rule.IsReactive) return RuleType.ReActiveRule;
            if (rule.IsExceptionHandler) return RuleType.ExceptionHandlerRule;

            return RuleType.None;
        }

        public enum RuleType
        {
            None,
            ProActiveRule,
            ReActiveRule,
            ExceptionHandlerRule
        }
    }
}
