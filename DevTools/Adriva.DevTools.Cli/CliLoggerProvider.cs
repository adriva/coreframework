using Microsoft.Extensions.Logging;

namespace Adriva.DevTools.Cli
{
    internal class CliLoggerProvider : ILoggerProvider
    {
        private readonly CliLogger Logger = new CliLogger();

        public ILogger CreateLogger(string categoryName) => this.Logger;

        public void Dispose()
        {

        }
    }
}
