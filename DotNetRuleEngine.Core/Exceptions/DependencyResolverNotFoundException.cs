using System;
using System.Runtime.Serialization;

namespace DotNetRuleEngine.Core.Exceptions
{
    [Serializable]
    public class DependencyResolverNotFoundException : Exception
    {
        public DependencyResolverNotFoundException()
        {
        }

        public DependencyResolverNotFoundException(string message) : base(message)
        {
        }

        public DependencyResolverNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DependencyResolverNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}