using System;

namespace Adriva.Extensions.Reporting.Abstractions
{
    public sealed class DataSourceRegistrationOptions
    {
        public RuntimeTypeHandle TypeHandle { get; private set; }

        public void UseType(RuntimeTypeHandle dataSourceTypeHandle)
        {
            this.TypeHandle = dataSourceTypeHandle;
        }
    }
}