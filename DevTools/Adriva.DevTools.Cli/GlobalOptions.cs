using System.CommandLine;

namespace Adriva.DevTools.Cli
{
    internal static class GlobalOptions
    {
        private static readonly Option VerboseOptionField = new Option<bool>(new[] { "-v", "--verbose" }, "Turns on verbose output.") { IsRequired = false };

        public static Option VerboseOption => GlobalOptions.VerboseOptionField;
    }
}
