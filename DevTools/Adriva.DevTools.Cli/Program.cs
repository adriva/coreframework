using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Adriva.DevTools.Cli
{
    class Program
    {
        private const string AppName = "Adriva Command Line Developer Tools";

        private static IEnumerable<MethodInfo> GetHandlerMethods()
        {
            var currentAssembly = Assembly.GetCallingAssembly();
            return from type in currentAssembly.GetTypes()
                   from method in type.GetMethods()
                   where type.IsClass
                            && !type.IsAbstract
                            && !type.IsGenericType
                            && !method.IsStatic
                            &&
                                (
                                    method.Name.Equals("Invoke", StringComparison.OrdinalIgnoreCase)
                                    || method.Name.Equals("InvokeAsync", StringComparison.OrdinalIgnoreCase)
                                )
                            &&
                                (
                                    null != method.GetCustomAttribute<CommandHandlerAttribute>()
                                )
                   select method;
        }

        private static Command CreateCommand(IServiceProvider serviceProvider, MethodInfo methodInfo)
        {
            if (null == methodInfo)
            {
                return null;
            }

            CommandHandlerAttribute commandHandlerAttribute = methodInfo.GetCustomAttribute<CommandHandlerAttribute>();

            Command command = new Command(commandHandlerAttribute.Name);

            var commandArgumentAttributes = methodInfo.GetCustomAttributes<CommandArgumentAttribute>();

            foreach (var commandArgumentAttribute in commandArgumentAttributes)
            {
                Option commandOption = null;

                if (null == commandArgumentAttribute.Type)
                {
                    commandOption = new Option(commandArgumentAttribute.Name, commandArgumentAttribute.Description)
                    {
                        IsRequired = commandArgumentAttribute.IsRequired,
                        IsHidden = commandArgumentAttribute.IsHidden,
                        Arity = ArgumentArity.ExactlyOne,
                    };
                }
                else
                {
                    commandOption = new Option(commandArgumentAttribute.Name, commandArgumentAttribute.Description, commandArgumentAttribute.Type)
                    {
                        IsRequired = commandArgumentAttribute.IsRequired,
                        IsHidden = commandArgumentAttribute.IsHidden
                    };
                }

                if (null != commandArgumentAttribute.Aliases)
                {
                    foreach (string alias in commandArgumentAttribute.Aliases)
                    {
                        commandOption.AddAlias(alias);
                    }
                }

                command.AddOption(commandOption);
            }

            var commandTypeInstance = ActivatorUtilities.CreateInstance(serviceProvider, methodInfo.DeclaringType);

            command.Handler = CommandHandler.Create(methodInfo, commandTypeInstance);

            return command;
        }

        static async Task<int> Main(string[] args)
        {
            Startup startup = new Startup();

            ServiceCollection services = new ServiceCollection();
            startup.ConfigureServices(services);
            IServiceProvider serviceProvider = services.BuildServiceProvider();

            RootCommand rootCommand = new RootCommand(Program.AppName);
            rootCommand.AddGlobalOption(new Option<bool>(new[] { "-v", "--verbose" }, "Turns on verbose output.") { IsRequired = false, });

            CommandLineBuilder commandLineBuilder = new CommandLineBuilder(rootCommand);

            foreach (var method in Program.GetHandlerMethods())
            {
                Command command = Program.CreateCommand(serviceProvider, method);

                if (null != command)
                {
                    rootCommand.AddCommand(command);
                }
            }

            Parser parser = commandLineBuilder
                                    .UseDefaults()
                                    .UseExceptionHandler((exception, context) =>
                                    {
                                        var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(string.Empty);
                                        logger.LogError(exception, "Failed");
                                    })
                                    .AddMiddleware(context =>
                                    {
                                        if (0 == context.ParseResult.Errors.Count)
                                        {
                                            context.Console.WriteLine(string.Empty);
                                            context.Console.WriteLine(Program.AppName);
                                            context.Console.WriteLine(string.Empty);
                                        }
                                    })
                                    .Build();
            return await parser.InvokeAsync(args);
        }
    }
}
