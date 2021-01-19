namespace Adriva.Extensions.Worker
{
    public interface IWorkerHostBuilder
    {
        IWorkerHostBuilder UseStartup<TClass>() where TClass : class;

        WorkerHost Build();
    }
}
