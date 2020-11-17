using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public class DataColumn
    {
        public string Name { get; private set; }

        public TypeCode DataType { get; private set; }

        public DataColumn(string name, TypeCode dataType = TypeCode.Object)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException("DataColumn requires a name.");
            if (TypeCode.Empty == dataType) throw new ArgumentException("DataType of a DataColumn cannot be TypeCode.Empty.");

            this.Name = name;
            this.DataType = dataType;
        }
    }
}