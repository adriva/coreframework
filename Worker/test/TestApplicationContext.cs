using System.Diagnostics.CodeAnalysis;
using Adriva.Extensions.Worker.Abstractions;
using Adriva.Extensions.Worker.Durable;
using DurableTask.Core;
using Microsoft.Extensions.DependencyInjection;

namespace test;

public sealed class TestApplicationContext
{
    private static readonly Lock SingletonLock = new();

    private static TestApplicationContext? StaticInstance;

    [NotNull]
    public IServiceProvider? ServiceProvider { get; private set; }

    public static TestApplicationContext Current
    {
        get
        {
            if (TestApplicationContext.StaticInstance is null)
            {
                TestApplicationContext.SingletonLock.Enter();

                if (TestApplicationContext.StaticInstance is null)
                {
                    TestApplicationContext.StaticInstance ??= new();
                    TestApplicationContext.StaticInstance.Initialize();
                }

                TestApplicationContext.SingletonLock.Exit();
            }

            return TestApplicationContext.StaticInstance;
        }
    }

    private TestApplicationContext()
    {

    }

    private void Initialize()
    {
        ServiceCollection services = [];

        services.AddWorkerHost(builder =>
        {
            builder
                .UseSqlServerDurableHost("Server=10.255.1.127\\SQL2017,1435;Database=Portal_UAT;User Id=poasPortal;Password=portal12;MultipleActiveResultSets=True;Encrypt=False", "test-app", "dtx");

            // builder
            //     .UseSqlServerHangfireHost("Server=10.255.1.127\\SQL2017,1435;Database=Portal_UAT;User Id=poasPortal;Password=portal12;MultipleActiveResultSets=True;Encrypt=False", "htx")
            //     .WithPlatformOptions(options =>
            //     {
            //         options.Queues = ["dev"];
            //         options.WorkerCount = 1;
            //     })
            //     .WithSerializerSettings(settings =>
            //     {
            //         settings.Formatting = Newtonsoft.Json.Formatting.None;
            //     })
            //     ;
        });

        this.ServiceProvider = services.BuildServiceProvider();
    }
}
