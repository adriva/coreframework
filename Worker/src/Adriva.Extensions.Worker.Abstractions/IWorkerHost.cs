using System.Linq.Expressions;

namespace Adriva.Extensions.Worker.Abstractions;

public interface IWorkerHost
{
    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}

public interface IWorkerClient
{
    Task<string> Enqueue<T>(Expression<Func<T, Task>> expression);
}
