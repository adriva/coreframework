using System;
using System.Diagnostics;
using Adriva.Common.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Adriva.Extensions.Reporting.Abstractions
{
    [DebuggerDisplay("FieldDefinition = {Name}")]
    public sealed class FieldDefinition : IDynamicDefinition, ICloneable<FieldDefinition>
    {
        public string Name { get; set; }

        public string DisplayName { get; set; }

        public FieldProperties Properties { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public TypeCode DataType { get; set; } = TypeCode.String;

        public string Format { get; set; }

        public JToken Options { get; set; }

        public FieldDefinition Clone()
        {
            var clone = new FieldDefinition()
            {
                Name = this.Name,
                DisplayName = this.DisplayName,
                Properties = this.Properties,
                DataType = this.DataType,
                Format = this.Format,
                Options = this.Options?.DeepClone()
            };

            return clone;
        }
    }
}