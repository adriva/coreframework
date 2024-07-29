using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Adriva.Storage.Azure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class PartitionKeyAttribute : Attribute
    {

    }
}
