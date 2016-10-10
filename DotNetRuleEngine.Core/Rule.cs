﻿using System;
using System.Linq;
using DotNetRuleEngine.Core.Interface;
using System.Collections.Generic;
using DotNetRuleEngine.Core.Models;
using DotNetRuleEngine.Core.Services;

namespace DotNetRuleEngine.Core
{
    public abstract class Rule<T> : IRule<T> where T : class, new()
    {
        private IList<object> Rules { get; set; } = new List<object>();

        public T Model { get; set; }

        public bool IsNested => Rules.Any();

        public bool IsReactive { get; set; }

        public bool IsPreactive { get; set; }

        public Type ObserveRule { get; set; }

        public bool IsExceptionHandler { get; set; }

        public Exception UnhandledException { get; set; }

        public IDependencyResolver Resolve { get; set; }

        public IConfiguration<T> Configuration { get; set; } = new Configuration<T>();

        public object TryGetValue(string key, int timeoutInMs = RuleDataService.DefaultTimeoutInMs) =>
            RuleDataService.GetInstance().GetValue(key, Configuration);

        public void TryAdd(string key, object value) =>
            RuleDataService.GetInstance().AddOrUpdate(key, value, Configuration);

        public IList<object> GetRules() => Rules;

        public void AddRules(params object[] rules) => Rules = rules;

        public virtual void Initialize() { }

        public virtual void BeforeInvoke() { }

        public virtual void AfterInvoke() { }

        public abstract IRuleResult Invoke();
    }
}
