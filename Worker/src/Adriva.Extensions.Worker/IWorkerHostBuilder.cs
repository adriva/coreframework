using Microsoft.Extensions.Hosting;

namespace Adriva.Extensions.Worker
{
    public interface IWorkerHostBuilder
    {
        IHostBuilder HostBuilder { get; }

        IWorkerHostBuilder UseStartup<TClass>() where TClass : class;

        WorkerHost Build();
    }
}
