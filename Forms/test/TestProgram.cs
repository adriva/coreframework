using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace test;

[TestClass]
public class TestProgram
{
    public static IServiceProvider? ServiceProvider;

    [AssemblyInitialize]
    public static void InitializeAssembly(TestContext context)
    {
        Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));

        ServiceCollection services = new();
        services
            .AddDynamicForms()
            ;

        TestProgram.ServiceProvider = services.BuildServiceProvider();
    }
}
