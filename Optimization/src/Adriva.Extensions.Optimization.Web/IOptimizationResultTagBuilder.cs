using System.Threading.Tasks;
using Adriva.Extensions.Optimization.Abstractions;
using Microsoft.AspNetCore.Html;

namespace Adriva.Extensions.Optimization.Web
{
    /// <summary>
    /// Provides methods to map optimization results to Html tags.
    /// </summary>
    public interface IOptimizationResultTagBuilder
    {
        /// <summary>
        /// Gets a value representing the supported extension by this tag builder.
        /// </summary>
        /// <value></value>
        string Extension { get; }

        Task PopulateHtmlTagAsync(ITagBuilderOptions options, Asset asset, IHtmlContentBuilder output);
    }
}
