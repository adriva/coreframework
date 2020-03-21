using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adriva.Extensions.Optimization.Abstractions
{
    public interface ITransform
    {
        Task<IEnumerable<Asset>> TransformAsync(params Asset[] assets);
    }
}