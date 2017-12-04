using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Extensions;
using DotNetRuleEngine.Test.AsyncRules;
using DotNetRuleEngine.Test.Models;
using DotNetRuleEngine.Test.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace DotnetRuleEngine.Test
{
    [TestClass]
    public class TestRxRule
    {
        [TestMethod]
        public void TestReactiveRules()
        {
            var product = new Product();
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
            ruleEngineExecutor.AddRules(new ProductRule(), new ProductReactiveRule());
            var rr = ruleEngineExecutor.Execute();
            Assert.IsTrue(rr.FindRuleResult<ProductReactiveRule>().Data["Ticks"].To<long>() >= rr.FindRuleResult<ProductRule>().Data["Ticks"].To<long>(),
                $"expected {rr.FindRuleResult<ProductReactiveRule>().Data["Ticks"]} actual {rr.FindRuleResult<ProductRule>().Data["Ticks"]}");
        }

        [TestMethod]
        public void TestPreactiveRules()
        {
            var product = new Product();
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
            ruleEngineExecutor.AddRules(new ProductRule(), new ProductPreactiveRule());
            var rr = ruleEngineExecutor.Execute();
            Assert.IsTrue(rr.FindRuleResult<ProductPreactiveRule>().Data["Ticks"].To<long>() <= rr.FindRuleResult<ProductRule>().Data["Ticks"].To<long>(),
                $"expected {rr.FindRuleResult<ProductPreactiveRule>().Data["Ticks"]} actual {rr.FindRuleResult<ProductRule>().Data["Ticks"]}");
        }

        [TestMethod]
        public void TestExceptionHandler()
        {
            var product = new Product();
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
            ruleEngineExecutor.AddRules(new ProductExceptionHandler(), new ProductExceptionThrown());
            var rr = ruleEngineExecutor.Execute();
            Assert.AreEqual(1, rr.Length);
            Assert.IsNotNull(rr.FindRuleResult<ProductExceptionHandler>().Error.Exception);
        }

        [TestMethod]
        public void TestExceptionHandlerAsync()
        {
            var product = new Product();
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
            ruleEngineExecutor.AddRules(new ProductExceptionHandlerAsync(), new ProductExceptionThrownAsync());
            var rr = ruleEngineExecutor.ExecuteAsync().Result;
            Assert.AreEqual(1, rr.Length);
            Assert.IsNotNull(rr.FindRuleResult<ProductExceptionHandlerAsync>().Error.Exception);
        }

        [TestMethod]
        public void TestGlobalExceptionHandler()
        {
            var product = new Product();
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
            ruleEngineExecutor.AddRules(new ProductGlobalExceptionHandler(), new ProductExceptionThrown());
            var rr = ruleEngineExecutor.Execute();
            Assert.AreEqual(1, rr.Length);
            Assert.IsNotNull(rr.FindRuleResult<ProductGlobalExceptionHandler>().Error.Exception);
        }

        [TestMethod]
        public void TestGlobalExceptionHandlerAsync()
        {
            var product = new Product();
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
            ruleEngineExecutor.AddRules(new ProductGlobalExceptionHandlerAsync(), new ProductExceptionThrownAsync());
            var rr = ruleEngineExecutor.ExecuteAsync().Result;
            Assert.AreEqual(1, rr.Length);
            Assert.IsNotNull(rr.FindRuleResult<ProductGlobalExceptionHandlerAsync>().Error.Exception);
        }

        [TestMethod]
        public void TestGlobalExceptionHandlerParallelAsync()
        {
            var product = new Product();
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
            ruleEngineExecutor.AddRules(new ProductGlobalExceptionHandlerAsync { IsParallel = true }, new ProductExceptionThrownAsync { IsParallel = true });
            var rr = ruleEngineExecutor.ExecuteAsync().Result;
            Assert.AreEqual(1, rr.Length);
            Assert.IsNotNull(rr.FindRuleResult<ProductGlobalExceptionHandlerAsync>().Error.Exception);
        }
    }
}
