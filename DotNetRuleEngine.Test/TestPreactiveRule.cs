using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Extensions;
using DotNetRuleEngine.Test.Models;
using DotNetRuleEngine.Test.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class TestPreactiveRule
    {
        [TestMethod]
        public void TestPreactiveRules()
        {
            var product = new Product();
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
            ruleEngineExecutor.AddRules(new ProductRule(), new ProductPreactiveRule());
            var rr = ruleEngineExecutor.Execute();
            Assert.IsTrue(rr.FindRuleResult<ProductPreactiveRule>().Data["Ticks"].To<long>() < rr.FindRuleResult<ProductRule>().Data["Ticks"].To<long>(),
                $"expected {rr.FindRuleResult<ProductPreactiveRule>().Data["Ticks"]} actual {rr.FindRuleResult<ProductRule>().Data["Ticks"]}");
        }
    }
}
