using System;

namespace Adriva.Web.Controls.Abstractions
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class DefaultNameAttribute : Attribute
    {
        public string Name { get; private set; }

        public DefaultNameAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));

            this.Name = name;
        }
    }
}