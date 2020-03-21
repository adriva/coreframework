namespace Adriva.Extensions.Optimization.Web
{
    public interface IOptimizationResultTagBuilderFactory
    {
        IOptimizationResultTagBuilder GetBuilder(string extension);
    }
}
