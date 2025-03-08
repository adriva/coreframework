using System;

namespace Adriva.Storage.Azure
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ETagAttribute : Attribute
    {

    }
}
