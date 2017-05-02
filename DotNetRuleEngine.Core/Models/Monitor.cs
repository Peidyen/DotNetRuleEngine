using Newtonsoft.Json;

namespace DotNetRuleEngine.Core.Models
{
    public class RuleModel<T>
    {
        public RuleModel(T model)
        {
            JsonModel = JsonConvert.SerializeObject(model);
        }
        public string JsonModel { get; }

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
