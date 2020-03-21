using System.Threading.Tasks;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface IOptimizationManager
    {
        Task<OptimizationResult> OptimizeAsync(IOptimizationContext context, string extension);

    }
}