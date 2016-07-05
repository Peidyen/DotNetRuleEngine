using System;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core.Models
{
    public class Error : IError
    {
        public string Message { get; set; }

        public Exception Exception { get; set; }
    }
}
