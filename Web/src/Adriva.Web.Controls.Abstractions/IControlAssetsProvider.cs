using System.Collections.Generic;

namespace Adriva.Web.Controls.Abstractions
{
    public interface IControlAssetsProvider
    {
        IEnumerable<string> GetJavaScriptUrls();

        IEnumerable<string> GetStyleSheetUrls();
    }
}