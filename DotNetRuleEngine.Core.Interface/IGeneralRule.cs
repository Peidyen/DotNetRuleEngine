using System;
using System.Collections.Generic;

namespace DotNetRuleEngine.Core.Interface
{
    public interface IGeneralRule<T> where T : class, new()
    {
        T Model { get; set; }

        bool IsNested { get; }

        bool IsReactive { get; set; }

        bool IsPreactive { get; set; }

        Type ObserveRule { get; }

        IDependencyResolver Resolve { get; set; }

        IConfiguration<T> Configuration { get; set;  }

        ICollection<IGeneralRule<T>> GetRules();

        void AddRules(params IGeneralRule<T>[] rules);
    }
}