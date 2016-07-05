using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Extensions;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;
using DotNetRuleEngine.Test.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class ProductRuleAsync : RuleAsync<Product>
    {
        public async override Task BeforeInvokeAsync()
        {
            await TryAddAsync("Description", Task.FromResult<object>("Description"));
        }

        public override async Task<IRuleResult> InvokeAsync()
        {
            var description = TryGetValueAsync("Description").Result.To<string>();
            Model.Description = $"Product {description}";            

            return await RuleResult.CreateAsync(new RuleResult
            {
                Name = "ProductRule",
                Result = Model.Description,
                Data = { { "Description", description } }
            });
        }
    }
}