using System;

namespace Adriva.Storage.Abstractions
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class NotMappedAttribute : Attribute
    {

    }
}