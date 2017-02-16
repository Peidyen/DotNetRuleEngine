using System.Threading;
using System.Threading.Tasks;
using DotNetRuleEngine.Core.Interface;

namespace DotNetRuleEngine.Core.Models
{
    public class ParallelConfiguration<T> : IParellelConfiguration<T> where T : class, new()
    {        
        public TaskCreationOptions TaskCreationOptions { get; set; } = TaskCreationOptions.None;

        public CancellationTokenSource CancellationTokenSource { get; set; }        

        public TaskScheduler TaskScheduler { get; set; } = TaskScheduler.Default;
    }
}