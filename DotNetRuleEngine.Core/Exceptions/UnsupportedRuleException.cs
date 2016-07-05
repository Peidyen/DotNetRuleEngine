using System;
using System.Runtime.Serialization;

namespace DotNetRuleEngine.Core.Exceptions
{
    [Serializable]
    public class UnsupportedRuleException : Exception
    {
        public UnsupportedRuleException()
        {
        }

        public UnsupportedRuleException(string message) : base(message)
        {
        }

        public UnsupportedRuleException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnsupportedRuleException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}