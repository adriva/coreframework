using System.Collections.Generic;
using System.Threading.Tasks;

namespace Adriva.Web.Controls.Abstractions
{
    public interface IControlRendererEvents
    {
        Task<IEnumerable<string>> OnAssetPathsResolved(IEnumerable<string> assetPaths);
    }
}