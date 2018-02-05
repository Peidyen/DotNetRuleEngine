using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Extensions;
using DotNetRuleEngine.Test.Models;
using DotNetRuleEngine.Test.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class TestProactiveRule
    {
        [TestMethod]
        public void TestProactiveRules()
        {
            var product = new Product();
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
            ruleEngineExecutor.AddRules(new ProductRule(), new ProductProactiveRule());
            var rr = ruleEngineExecutor.Execute();
            Assert.IsTrue(rr.FindRuleResult<ProductProactiveRule>().Data["Ticks"].To<long>() < rr.FindRuleResult<ProductRule>().Data["Ticks"].To<long>(),
                $"expected {rr.FindRuleResult<ProductProactiveRule>().Data["Ticks"]} actual {rr.FindRuleResult<ProductRule>().Data["Ticks"]}");
        }
    }
}
