using System;
using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    internal class ProductGlobalExceptionHandlerAsync : RuleAsync<Product>
    {
        public override Task InitializeAsync()
        {
            IsGlobalExceptionHandler = true;
            return base.InitializeAsync();
        }

        public override Task<IRuleResult> InvokeAsync()
        {
            var ruleResult = new RuleResult();

            if (UnhandledException?.GetType() == typeof(Exception))
            {
                ruleResult.Error = new Error { Exception = UnhandledException };
            }

            return RuleResult.CreateAsync(ruleResult);
        }
    }
}
