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
        private IList<object> Rules { get; } = new List<object>();

        public T Model { get; set; }

        public bool IsNested => Rules.Any();

        public bool IsReactive { get; set; }

        public bool IsProactive { get; set; }

        public Type ObserveRule { get; set; }

        public bool IsExceptionHandler { get; set; }

        public bool IsGlobalExceptionHandler { get; set; }

        public Exception UnhandledException { get; set; }

        public IDependencyResolver Resolve { get; set; }

        public IConfiguration<T> Configuration { get; set; } = new Configuration<T>();

        public object TryGetValue(string key, int timeoutInMs = DataSharingService.DefaultTimeoutInMs) =>
            DataSharingService.GetInstance().GetValue(key, Configuration);

        public void TryAdd(string key, object value) =>
            DataSharingService.GetInstance().AddOrUpdate(key, value, Configuration);

        public IList<object> GetRules() => Rules;

        public void AddRules(params object[] rules)
        {
            foreach (var rule in rules)
            {
                Rules.Add(rule);
            }
        }

        public virtual void Initialize() { }

        public virtual void BeforeInvoke() { }

        public virtual void AfterInvoke() { }

        public abstract IRuleResult Invoke();
    }
}
