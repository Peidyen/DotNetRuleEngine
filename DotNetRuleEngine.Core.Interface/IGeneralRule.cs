﻿using System;
using System.Collections.Generic;

namespace DotNetRuleEngine.Core.Interface
{
    public interface IGeneralRule<T> where T : class, new()
    {
        T Model { get; set; }

        bool IsNested { get; }

        bool IsReactive { get; set; }

        bool IsProactive { get; set; }

        bool IsExceptionHandler { get; set; }

        bool IsGlobalExceptionHandler { get; set; }

        Type ObserveRule { get; }

        Exception UnhandledException { get; set; }

        IDependencyResolver Resolve { get; set; }

        IConfiguration<T> Configuration { get; set; }

        IList<object> GetRules();

        void AddRules(params object[] rules);
    }
}