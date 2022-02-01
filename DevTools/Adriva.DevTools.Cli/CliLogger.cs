using System;
using Microsoft.Extensions.Logging;

namespace Adriva.DevTools.Cli
{
    internal sealed class CliLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return new Adriva.Common.Core.NullDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine(formatter.Invoke(state, exception));
        }
    }
}
