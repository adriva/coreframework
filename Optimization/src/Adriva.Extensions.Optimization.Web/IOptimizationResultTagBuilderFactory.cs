namespace Adriva.Extensions.Optimization.Web
{
    /// <summary>
    /// Provides methods to get or create a concrete implementation of Adriva.Extensions.Optimization.Web.IOptimizationResultTagBuilder for a given asset extension.
    /// </summary>
    public interface IOptimizationResultTagBuilderFactory
    {
        /// <summary>
        /// Gets or creates a concrete implementation of Adriva.Extensions.Optimization.Web.IOptimizationResultTagBuilder interface that will be used to build html tags for the given asset extension.
        /// </summary>
        /// <param name="extension">The extension of the assets that the returned tag builder can process.</param>
        /// <returns>A concrete implementation of Adriva.Extensions.Optimization.Web.IOptimizationResultTagBuilder interface that will be used to generate html tags.</returns>
        IOptimizationResultTagBuilder GetBuilder(string extension);
    }
}
