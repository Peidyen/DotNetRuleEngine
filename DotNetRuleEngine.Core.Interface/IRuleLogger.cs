using System;

namespace DotNetRuleEngine.Core.Interface
{
    public interface IRuleLogger
    {
        void Write<T>(Guid ruleEngineId, T model);
    }
}
