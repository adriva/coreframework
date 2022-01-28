using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class EnumDataSource : IDataSource
    {
        private Type EnumType;

        public Task OpenAsync(DataSourceDefinition dataSourceDefinition)
        {
            if (string.IsNullOrWhiteSpace(dataSourceDefinition?.ConnectionString))
            {
                throw new ArgumentException(nameof(dataSourceDefinition.ConnectionString), "Enum connection string must point to a valid Enum type.");
            }

            this.EnumType = Type.GetType(dataSourceDefinition.ConnectionString, false, true);

            if (null == this.EnumType)
            {
                throw new InvalidOperationException($"Specified data source Enum '{dataSourceDefinition.ConnectionString}' could not be found or loaded.");
            }

            return Task.CompletedTask;
        }


        public Task<DataSet> GetDataAsync(ReportCommand command, FieldDefinition[] fields)
        {
            if (null == fields || 2 != fields.Length)
            {
                fields = new FieldDefinition[] {
                    new FieldDefinition(){Name = "Value"},
                    new FieldDefinition(){Name = "Text"}
                };
            }

            Type enumBaseType = this.EnumType.GetEnumUnderlyingType();
            string[] names = this.EnumType.GetEnumNames();

            var dataset = DataSet.FromFields(fields);

            foreach (var name in names)
            {
                var value = Convert.ChangeType(Enum.Parse(this.EnumType, name), enumBaseType);

                string description = null;

                var memberInfo = this.EnumType.GetMember(name)?.FirstOrDefault();

                if (null != memberInfo)
                {
                    var descriptionAttribute = memberInfo.GetCustomAttribute<DescriptionAttribute>();
                    description = descriptionAttribute?.Description;

                    if (string.IsNullOrWhiteSpace(description))
                    {
                        var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
                        description = displayAttribute?.Name;
                    }
                }

                var row = dataset.CreateRow();
                row.AddData(value);
                row.AddData(string.IsNullOrWhiteSpace(description) ? name : description);
            }

            return Task.FromResult(dataset);
        }

        public Task CloseAsync() => Task.CompletedTask;
    }
}