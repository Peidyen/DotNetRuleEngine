using System;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    internal class ProductExceptionThrown : Rule<Product>
    {
        public override IRuleResult Invoke()
        {
            throw new Exception();
        }
    }
}