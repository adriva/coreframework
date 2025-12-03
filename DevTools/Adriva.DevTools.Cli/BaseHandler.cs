using System;
using Microsoft.Extensions.Logging;

namespace Adriva.DevTools.Cli
{
    internal abstract class BaseHandler
    {
        protected ILogger Logger { get; private set; }

        protected BaseHandler(ILoggerFactory loggerFactory)
        {
            this.Logger = loggerFactory.CreateLogger(string.Empty);
        }

        protected void RunWithStepOver(Action action, string message)
        {
            if (null == action) return;

            try
            {
                action();
            }
            catch (Exception error)
            {
                if (Context.Current.ShouldStepOverErrors)
                {
                    this.Logger.LogError(error, $"{message}. -so flag is set set this error will be ignored.");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
