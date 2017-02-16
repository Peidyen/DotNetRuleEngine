using System.Threading;
using System.Threading.Tasks;

namespace DotNetRuleEngine.Core.Interface
{
    public interface IParellelConfiguration<T> where T : class, new()
    {
        TaskCreationOptions TaskCreationOptions { get; set; }

        CancellationTokenSource CancellationTokenSource { get; set; }

        TaskScheduler TaskScheduler { get; set; }
    }
}