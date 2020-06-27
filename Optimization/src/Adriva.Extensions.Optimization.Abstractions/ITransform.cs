using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface ITransform
    {
        Task<IEnumerable<Asset>> TransformAsync(string extension, params Asset[] assets);

        ValueTask CleanUpAsync(params Asset[] assets);
    }
}