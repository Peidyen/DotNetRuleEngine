using System;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    internal class ProductPreactiveRule : Rule<Product>
    {
        public override void Initialize()
        {
            IsPreactive = true;
            ObserveRule = typeof(ProductRule);
        }

        public override IRuleResult Invoke()
        {
            TryAdd("Ticks", DateTime.Now.Ticks);
            return new RuleResult { Result = Model.Description, Data = { { "Ticks", TryGetValue("Ticks") } } };

        }
    }
}
