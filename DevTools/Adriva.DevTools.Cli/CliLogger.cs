using System;
using Microsoft.Extensions.Logging;

namespace Adriva.DevTools.Cli
{
    internal sealed class CliLogger : ILogger
    {
        private object SyncLock = new object();

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
            lock (this.SyncLock)
            {
                ConsoleColor defaultColor = Console.ForegroundColor;
                ConsoleColor color = Console.ForegroundColor;

                switch (logLevel)
                {
                    case LogLevel.Warning:
                        color = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        color = ConsoleColor.Red;
                        break;
                }

                Console.ForegroundColor = color;
                Console.WriteLine(formatter.Invoke(state, exception));

                if (null != exception)
                {
                    if (Context.Current.IsVerbose)
                    {
                        Console.Error.WriteLine(exception.ToString());
                    }
                    else
                    {
                        Console.Error.WriteLine(exception.Message);
                    }
                }

                Console.ForegroundColor = defaultColor;
            }
        }
    }
}
