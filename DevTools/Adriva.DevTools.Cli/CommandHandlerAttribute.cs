using System;

namespace Adriva.DevTools.Cli
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal sealed class CommandHandlerAttribute : Attribute
    {
        public string Name { get; private set; }

        public CommandHandlerAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException(nameof(name));
            }

            this.Name = name;
        }
    }
}
