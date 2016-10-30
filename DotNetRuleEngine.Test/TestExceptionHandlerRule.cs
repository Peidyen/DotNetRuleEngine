using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Extensions;
using DotNetRuleEngine.Test.Models;
using DotNetRuleEngine.Test.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace DotNetRuleEngine.Test
{
    [TestClass]
    public class TestExceptionHandlerRule
    {
        [TestMethod]
        public void TestExceptionHandler()
        {
            var product = new Product();
            var ruleEngineExecutor = RuleEngine<Product>.GetInstance(product);
            ruleEngineExecutor.AddRules(new ProductExceptionHandler(), new ProductExceptionThrown());
            var rr = ruleEngineExecutor.Execute();
            Assert.IsNotNull(rr.FindRuleResult<ProductExceptionHandler>().Error.Exception);
        }
    }
}
