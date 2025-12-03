using System.CommandLine;

namespace Adriva.DevTools.Cli
{
    internal static class GlobalOptions
    {
        private static readonly Option VerboseOptionField = new Option<bool>(new[] { "-v", "--verbose" }, "Turns on verbose output.") { IsRequired = false };

        private static readonly Option StepOverErrorsOptionField = new Option<bool>(new[] { "-so", "--stepover" }, "Turns on step over errors flags, which causes the exceptions to be ignored and keeps running the given command.") { IsRequired = false };

        public static Option VerboseOption => GlobalOptions.VerboseOptionField;

        public static Option StepOverErrorsOption => GlobalOptions.StepOverErrorsOptionField;
    }
}
