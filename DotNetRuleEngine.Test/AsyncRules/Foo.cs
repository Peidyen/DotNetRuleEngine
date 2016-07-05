using System.Threading.Tasks;
using DotNetRuleEngine.Core;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;

namespace DotNetRuleEngine.Test.AsyncRules
{
    class Foo
    {
        public string Name { get; set; }
        public string Phone { get; set; }
    }

    class UpdatePhone : RuleAsync<Foo>
    {
        public override Task InitializeAsync()
        {
            IsParallel = true;
            return Task.FromResult<object>(null);
        }
        public override async Task<IRuleResult> InvokeAsync()
        {
            await Task.Delay(100);
            await TryAddAsync("PhoneNumber", Task.FromResult<object>(2064551002));
            return await Task.FromResult<IRuleResult>(null);
        }
    }


    class UpdateName : RuleAsync<Foo>
    {
        public override Task InitializeAsync()
        {
            AddRules(new UpdatePhone());
            Configuration.InvokeNestedRulesFirst = true;
            return Task.FromResult<object>(null);

        }
        public override async Task<IRuleResult> InvokeAsync()
        {
            var phone = await TryGetValueAsync("PhoneNumber");
            Model.Phone = phone?.ToString();

            return await Task.FromResult<IRuleResult>(new RuleResult { Result = Model });
        }
    }
}
