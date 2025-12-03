using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class EnumDataSource : ObjectDataSource
    {
        public EnumDataSource(IServiceProvider serviceProvider) : base(serviceProvider)
        {

        }

        public override Task<DataSet> GetDataAsync(ReportCommand command, FieldDefinition[] fields, JToken outputOptions)
        {
            if (null == fields || 2 != fields.Length)
            {
                fields = new FieldDefinition[] {
                    new(){Name = Constants.EnumValueField},
                    new(){Name = Constants.EnumTextField }
                };
            }

            Type enumBaseType = this.ObjectType.GetEnumUnderlyingType();
            string[] names = this.ObjectType.GetEnumNames();

            var dataset = DataSet.FromFields(fields);

            foreach (var name in names)
            {
                var value = Convert.ChangeType(Enum.Parse(this.ObjectType, name), enumBaseType);

                string description = null;

                var memberInfo = this.ObjectType.GetMember(name)?.FirstOrDefault();

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
    }
}