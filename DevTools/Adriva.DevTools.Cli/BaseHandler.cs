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
    }
}
