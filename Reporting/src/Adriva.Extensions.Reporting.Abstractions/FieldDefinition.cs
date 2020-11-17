using Adriva.Common.Core;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class FieldDefinition : ICloneable<FieldDefinition>
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public FieldDefinition Clone()
        {
            var clone = new FieldDefinition()
            {
                Name = this.Name,
                DisplayName = this.DisplayName
            };

            return clone;
        }
    }
}