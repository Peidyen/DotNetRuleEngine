using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.Rules
{
    class ProductSkip : Rule<Product>
    {
        public override void Initialize()
        {
            Configuration.Skip = true;
        }

        public override IRuleResult Invoke()
        {
            Model.Description = "Product Description";
            return new RuleResult { Name = "ProductRule", Result = Model.Description };
        }
    }
}