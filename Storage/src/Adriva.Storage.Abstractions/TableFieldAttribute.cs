using System;

namespace Adriva.Storage.Abstractions
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class TableFieldAttribute : Attribute
    {
        public string FieldName { get; private set; }

        public TableFieldAttribute(string fieldName)
        {
            if (string.IsNullOrWhiteSpace(fieldName)) throw new ArgumentNullException(nameof(fieldName));

            this.FieldName = fieldName;
        }
    }
}