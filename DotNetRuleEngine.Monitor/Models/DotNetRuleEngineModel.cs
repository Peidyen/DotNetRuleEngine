using System;
using System.Collections.Generic;

namespace DotNetRuleEngine.Monitor.Models
{
    public class DotNetRuleEngineModel
    {
        public int DotNetRuleEngineModelId { get; set; }

        public Guid RuleEngineId { get; set; }

        public byte[] Timestamp { get; set; }

        public List<RuleModel> RuleModels { get; set; }
    }

    public class RuleModel
    {
        public int RuleModelId { get; set; }

        public int DotNetRuleEngineModelId { get; set; }

        public string JsonModel { get; set; }

        public string Rule { get; set; }

        public RuleType RuleType { get; set; } = RuleType.None;

        public string ObservingRule { get; set; }
    }

    public enum RuleType
    {
        None,
        PreActiveRule,
        ReActiveRule,
        ExceptionHandlerRule
    }
}
