using Adriva.Extensions.Optimization.Abstractions;

namespace Adriva.Extensions.Optimization.Web
{
    public interface ITagBuilderOptions
    {
        AssetFileExtension Extension { get; }

        OptimizationTagOutput Output { get; }

        string ContextName { get; }
    }
}
