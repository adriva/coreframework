using System.Diagnostics;
using Adriva.Common.Core;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [DebuggerDisplay("FieldDefinition = {Name}")]
    public sealed class FieldDefinition : IDynamicDefinition, ICloneable<FieldDefinition>
    {
        public string Name { get; internal set; }

        public string DisplayName { get; set; }

        public JToken Options { get; set; }

        public FieldDefinition Clone()
        {
            var clone = new FieldDefinition()
            {
                Name = this.Name,
                DisplayName = this.DisplayName,
                Options = this.Options?.DeepClone()
            };

            return clone;
        }
    }
}