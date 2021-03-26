using Adriva.Common.Core;
using Microsoft.Extensions.Configuration;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class FieldDefinition : ICloneable<FieldDefinition>
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public IConfigurationSection Options { get; set; }

        public FieldDefinition Clone()
        {
            var clone = new FieldDefinition()
            {
                Name = this.Name,
                DisplayName = this.DisplayName
            };

            if (null != this.Options)
            {
                clone.Options = this.Options.Get<IConfigurationSection>();
            }

            return clone;
        }
    }
}