using System;
using System.Globalization;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DefaultFilterValueBinder : IFilterValueBinder
    {
        public FilterValue GetFilterValue(FilterDefinition filterDefinition, string rawValue)
        {
            object value;

            if (filterDefinition.Properties.HasFlag(FilterProperties.Constant)) value = filterDefinition.DefaultValue;
            else if (filterDefinition.Properties.HasFlag(FilterProperties.Required) && null == rawValue)
                throw new ArgumentNullException(filterDefinition.Name, $"Filter parameter {filterDefinition.Name} is required.");
            else if (filterDefinition.Properties.HasFlag(FilterProperties.Context))
            {
                throw new NotImplementedException();
            }
            else
            {
                value = rawValue ?? filterDefinition.DefaultValue;
            }

            value = Convert.ChangeType(value, filterDefinition.DataType, CultureInfo.CurrentCulture);
            return new FilterValue(rawValue, value);
        }
    }
}