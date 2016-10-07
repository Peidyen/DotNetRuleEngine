using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Extensions;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core.Services
{
    internal sealed class ActiveRuleService<T> where T : class, new()
    {
        private readonly IEnumerable<IGeneralRule<T>> _rules;
        private readonly Lazy<ConcurrentDictionary<Type, IList<IGeneralRule<T>>>> _preactiveRules;
        private readonly Lazy<ConcurrentDictionary<Type, IList<IGeneralRule<T>>>> _reactiveRules;

        public ActiveRuleService(IEnumerable<IGeneralRule<T>> rules)
        {
            _rules = rules;
            _preactiveRules = new Lazy<ConcurrentDictionary<Type, IList<IGeneralRule<T>>>>(CreatePreactiveRules, true);
            _reactiveRules = new Lazy<ConcurrentDictionary<Type, IList<IGeneralRule<T>>>>(CreateReactiveRules, true);
        }

        public ConcurrentDictionary<Type, IList<IGeneralRule<T>>> GetReactiveRules() => _reactiveRules.Value;

        public ConcurrentDictionary<Type, IList<IGeneralRule<T>>> GetPreactiveRules() => _preactiveRules.Value;

        public IEnumerable<IGeneralRule<T>> FilterActivatingRules(IEnumerable<IGeneralRule<T>> rules) =>
            rules.Where(r => !r.IsReactive && !r.IsPreactive).ToList();

        private ConcurrentDictionary<Type, IList<IGeneralRule<T>>> CreatePreactiveRules()
        {
            var preactiveRules = new ConcurrentDictionary<Type, IList<IGeneralRule<T>>>();
            GetActivatingRules(_rules, preactiveRules, rule => rule.IsPreactive);

            return preactiveRules;
        }

        private ConcurrentDictionary<Type, IList<IGeneralRule<T>>> CreateReactiveRules()
        {
            var reactiveRules = new ConcurrentDictionary<Type, IList<IGeneralRule<T>>>();
            GetActivatingRules(_rules, reactiveRules, rule => rule.IsReactive);

            return reactiveRules;
        }

        private static void GetActivatingRules(IEnumerable<IGeneralRule<T>> rules,
            ConcurrentDictionary<Type, IList<IGeneralRule<T>>> activatingRules, Predicate<IGeneralRule<T>> predicate)
        {
            Parallel.ForEach(rules, r =>
            {
                if (predicate(r))
                {
                    activatingRules.AddOrUpdate(r.ObserveRule, new List<IGeneralRule<T>> { r }, (type, list) =>
                   {
                       list.Add(r);
                       return list;
                   });
                }
                if (r.IsNested) GetActivatingRules(r.GetResolvedRules(), activatingRules, predicate);
            });
        }
    }
}
