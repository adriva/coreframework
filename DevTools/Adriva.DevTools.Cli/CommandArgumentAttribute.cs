using System;

namespace Adriva.DevTools.Cli
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    internal sealed class CommandArgumentAttribute : Attribute
    {
        public string Name { get; private set; }

        public string[] Aliases { get; set; }

        public bool IsRequired { get; set; }

        public bool IsHidden { get; set; }

        public Type Type { get; set; }

        public CommandArgumentAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            this.Name = name;
        }
    }
}
