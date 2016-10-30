using System;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    internal class ProductExceptionHandler : Rule<Product>
    {
        public override void Initialize()
        {
            IsExceptionHandler = true;
            ObserveRule = typeof(ProductExceptionThrown);
        }

        public override IRuleResult Invoke()
        {
            var ruleResult = new RuleResult();

            if (UnhandledException?.GetType() == typeof(Exception))
            {
                ruleResult.Error = new Error { Exception = UnhandledException };
            }

            return ruleResult;
        }
    }
}
