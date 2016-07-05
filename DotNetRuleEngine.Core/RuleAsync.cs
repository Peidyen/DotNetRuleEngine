using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Interface;
using DotNetRuleEngine.Core.Models;
using DotNetRuleEngine.Core.Services;

namespace DotNetRuleEngine.Core
{
    public abstract class RuleAsync<T> : IRuleAsync<T> where T : class, new()
    {
        private IList<IGeneralRule<T>> Rules { get; set; } = new List<IGeneralRule<T>>();

        public T Model { get; set; }

        public bool IsParallel { get; set; }

        public bool IsNested => Rules.Any();

        public bool IsReactive { get; set; }

        public bool IsPreactive { get; set; }

        public Type ObserveRule { get; set; }

        public IDependencyResolver Resolve { get; set; }

        public IConfiguration<T> Configuration { get; set; } = new Configuration<T>();

        public async Task<object> TryGetValueAsync(string key, int timeoutInMs = RuleDataService.DefaultTimeoutInMs) => 
            await RuleDataService.GetInstance().GetValueAsync(key, Configuration, timeoutInMs);

        public async Task TryAddAsync(string key, Task<object> value) => 
            await RuleDataService.GetInstance().AddOrUpdateAsync(key, value, Configuration);

        public ICollection<IGeneralRule<T>> GetRules() => Rules;
        
        public void AddRules(params IGeneralRule<T>[] rules) => Rules = rules;

        public virtual async Task InitializeAsync() => await Task.FromResult<object>(null);

        public virtual async Task BeforeInvokeAsync() => await Task.FromResult<object>(null);

        public virtual async Task AfterInvokeAsync() => await Task.FromResult<object>(null);

        public abstract Task<IRuleResult> InvokeAsync();
    }
}
