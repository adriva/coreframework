using System;

namespace Adriva.Storage.Abstractions
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class TableFieldAttribute : Attribute
    {
        public string FieldName { get; private set; }

        public bool IsMapped { get; set; }

        public TableFieldAttribute(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException(nameof(fieldName));

            this.FieldName = fieldName;
            this.IsMapped = true;
        }

        public TableFieldAttribute()
        {
            this.FieldName = null;
            this.IsMapped = false;
        }
    }
}