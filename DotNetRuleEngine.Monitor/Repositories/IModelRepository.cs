using System;
using DotNetRuleEngine.Monitor.Domain;

namespace DotNetRuleEngine.Monitor.Repositories
{
    public interface IModelRepository
    {
        void Put(Guid id, Model model);

        RuleEngine Get(Guid ruleEngineId);
    }
}
