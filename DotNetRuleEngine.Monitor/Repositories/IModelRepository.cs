using System;
using DotNetRuleEngine.Monitor.Models;

namespace DotNetRuleEngine.Monitor.Repositories
{
    public interface IModelRepository
    {
        void Put(Guid id, RuleModel model);

        DotNetRuleEngineModel Get(Guid ruleEngineId);
    }
}
