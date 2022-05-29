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
            if (!Context.Current.IsVerbose)
            {
                if (LogLevel.Trace == logLevel || LogLevel.Debug == logLevel)
                {
                    return;
                }
            }

            lock (this.SyncLock)
            {
                string prefix = string.Empty;
                ConsoleColor defaultColor = Console.ForegroundColor;
                ConsoleColor color = Console.ForegroundColor;

                switch (logLevel)
                {
                    case LogLevel.Debug:
                    case LogLevel.Trace:
                        color = ConsoleColor.Cyan;
                        prefix = "\t";
                        break;
                    case LogLevel.Warning:
                        color = ConsoleColor.Yellow;
                        prefix = "[Warning] ";
                        break;
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        color = ConsoleColor.Red;
                        prefix = "[Error] ";
                        break;
                }

                Console.ForegroundColor = color;
                Console.WriteLine(prefix + formatter.Invoke(state, exception));

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
