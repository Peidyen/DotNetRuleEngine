using DotNetRuleEngine.Core;
using DotNetRuleEngine.Test.Models;
using DotNetRuleEngine.Test.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class TestReactiveRule
    {
        [TestMethod]
        public void TestReactiveRules()
        {
            var product = new Product();
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
            ruleEngineExecutor.AddRules(new ProductRule());
            var rr = ruleEngineExecutor.Execute();
            Assert.IsTrue(rr.FindRuleResult<ProductReactiveRule>().Data["Ticks"].To<long>() >= rr.FindRuleResult<ProductRule>().Data["Ticks"].To<long>(),
                $"expected {rr.FindRuleResult<ProductReactiveRule>().Data["Ticks"]} actual {rr.FindRuleResult<ProductRule>().Data["Ticks"]}");
        }
    }
}