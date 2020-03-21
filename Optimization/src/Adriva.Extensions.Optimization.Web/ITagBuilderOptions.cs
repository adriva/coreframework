namespace Adriva.Extensions.Optimization.Web
{
    public interface ITagBuilderOptions
    {
        string Extension { get; }

        OptimizationTagOutput Output { get; }
    }
}
