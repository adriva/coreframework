using Microsoft.Extensions.Logging;

namespace Adriva.DevTools.Cli.Dummy
{
    internal sealed class ChuckNorrisHandler
    {
        private readonly ILogger Logger;

        public ChuckNorrisHandler(ILogger<ChuckNorrisHandler> logger)
        {
            this.Logger = logger;
        }

        [CommandHandler("mrnorris")]
        public void Invoke(bool verbose)
        {
            if (!verbose)
            {
                this.Logger.LogWarning("Chuck Norris never uses tools, but only his own bare hands.");
            }
            else
            {
                this.Logger.LogTrace("Chuck Norris never uses verbose output. He always knows what is happening around.");
            }
        }
    }
}