namespace Adriva.Extensions.Reporting.Abstractions
{
    public interface IFilterValueBinder
    {
        FilterValue GetFilterValue(FilterDefinition filterDefinition, string rawValue);
    }
}