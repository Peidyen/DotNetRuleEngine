using System;
using System.Runtime.Serialization;

namespace DotNetRuleEngine.Core.Exceptions
{
    [Serializable]
    public class ModelInstanceNotFoundException : Exception
    {
        public ModelInstanceNotFoundException()
        {
        }

        public ModelInstanceNotFoundException(string message) : base(message)
        {
        }

        public ModelInstanceNotFoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ModelInstanceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
