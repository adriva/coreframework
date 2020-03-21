using System.Threading.Tasks;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface IOptimizationResultFormatter<TOutput>
    {
        string TargetExtension { get; }

        Task WriteAsync(TOutput output, OptimizationResult result);
    }
}