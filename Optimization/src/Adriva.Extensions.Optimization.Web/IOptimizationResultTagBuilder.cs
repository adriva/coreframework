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
        /// <value>A string value that represents the extension of the assets, which this tag builder can process.</value>
        string Extension { get; }

        /// <summary>
        /// Generates an html tag and populates the attributes if needed for the given asset.
        /// </summary>
        /// <param name="options">A concrete implementation of ITagBuilderOptions that this tag builder may use.</param>
        /// <param name="asset">The asset for which the html tag will be generated for.</param>
        /// <param name="output">A concrete implementation of IHtmlContentBuilder that the output will be written to.</param>
        /// <returns>Represents the asynchronous process operation.</returns>
        Task PopulateHtmlTagAsync(ITagBuilderOptions options, Asset asset, IHtmlContentBuilder output);
    }
}
