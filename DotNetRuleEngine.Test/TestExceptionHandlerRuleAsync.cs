using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Extensions;
using DotNetRuleEngine.Test.AsyncRules;
using DotNetRuleEngine.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class TestExceptionHandlerRuleAsync
    {
        [TestMethod]
        public void TestExceptionHandlerAsync()
        {
            var product = new Product();
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
            ruleEngineExecutor.AddRules(new ProductExceptionHandlerAsync(), new ProductExceptionThrownAsync());
            var rr = ruleEngineExecutor.ExecuteAsync().Result;
            Assert.IsNotNull(rr.FindRuleResult<ProductExceptionHandlerAsync>().Error.Exception);
        }
    }
}