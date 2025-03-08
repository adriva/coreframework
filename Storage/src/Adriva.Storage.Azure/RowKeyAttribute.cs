using System;

namespace Adriva.Storage.Azure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class RowKeyAttribute : Attribute
    {

    }
}
