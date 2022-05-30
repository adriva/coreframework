using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;

namespace Adriva.DevTools.Cli
{
    internal sealed class Context
    {
        private static readonly object SingletonLock = new object();
        private static Context StaticCurrent;

        public static Context Current => Context.StaticCurrent;

        public static Context Create(InvocationContext invocationContext)
        {
            if (null == Context.StaticCurrent)
            {
                lock (Context.SingletonLock)
                {
                    if (null == Context.StaticCurrent)
                    {
                        Context.StaticCurrent = new Context(invocationContext);
                    }
                }
            }

            return Context.StaticCurrent;
        }

        public InvocationContext InvocationContext { get; private set; }

        public bool IsVerbose => this.InvocationContext.ParseResult.HasOption(GlobalOptions.VerboseOption);

        public bool ShouldStepOverErrors => this.InvocationContext.ParseResult.HasOption(GlobalOptions.StepOverErrorsOption);

        private Context(InvocationContext invocationContext)
        {
            this.InvocationContext = invocationContext;
        }
    }
}
