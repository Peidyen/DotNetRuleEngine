using System;
using System.Runtime.Serialization;

namespace DotNetRuleEngine.Core.Exceptions
{
    [Serializable]
    public class UnsupportedRuleConfiguration : Exception
    {
        public UnsupportedRuleConfiguration()
        {
        }

        public UnsupportedRuleConfiguration(string message) : base(message)
        {
        }

        public UnsupportedRuleConfiguration(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnsupportedRuleConfiguration(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}