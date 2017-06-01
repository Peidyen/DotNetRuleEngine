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

    [Serializable]
    public class GlobalExceptionHandler : Exception
    {
        public GlobalExceptionHandler()
        {
        }

        public GlobalExceptionHandler(string message) : base(message)
        {
        }

        public GlobalExceptionHandler(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected GlobalExceptionHandler(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}