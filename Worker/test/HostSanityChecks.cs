using System.Data.SqlTypes;
using Adriva.Extensions.Worker.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace test;

public class Fafaf(int x)
{
    private int X = x;

    public int DoIt() => this.X * 9;
}

[TestClass]
public sealed class HostSanityChecks
{
    [Hangfire.Queue("dev")]
    public Task DoSomethingAsync(Fafaf f, int hashCode, string data)
    {
        System.Console.WriteLine($"DoSomethingAsync({f}, {hashCode}, {data})");
        return Task.CompletedTask;
    }

    private long GetTicks(HostSanityChecks sanityChecks)
    {
        return DateTime.Now.Ticks + sanityChecks.GetHashCode();
    }

    [TestMethod]
    public async Task InitializationChecks()
    {
        Assert.IsNotNull(TestApplicationContext.Current.ServiceProvider);

        CancellationTokenSource cancellationTokenSource = new();

        var workerHost = TestApplicationContext.Current.ServiceProvider.GetRequiredService<IWorkerHost>();
        var workerClient = TestApplicationContext.Current.ServiceProvider.GetRequiredService<IWorkerClient>();
        await workerHost.StartAsync(CancellationToken.None);
        Fafaf f = new(9);
        await workerClient.Enqueue<HostSanityChecks>(x => x.DoSomethingAsync(f, (2 * this.GetHashCode() > 0) ? 1 : -1, 5.ToString("N")));
        // await workerClient.Enqueue<SanityChecks>(x => x.DoSomethingAsync(0, 1, 2));
        await Task.Delay(60000);
        cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(35));
        await workerHost.StopAsync(cancellationTokenSource.Token);
    }
}
