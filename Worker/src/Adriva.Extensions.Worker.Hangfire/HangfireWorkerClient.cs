using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Adriva.Extensions.Worker.Abstractions;
using Hangfire;

namespace Adriva.Extensions.Worker.Hangfire;

public sealed class HangfireWorkerClient(IBackgroundJobClient backgroundJobClient) : IWorkerClient
{
    private readonly IBackgroundJobClient BackgroundJobClient = backgroundJobClient;

    public Task<string> Enqueue<T>(Expression<Func<T, Task>> expression)
    {
        string jobId = this.BackgroundJobClient.Enqueue(expression);
        return Task.FromResult(jobId);
    }
}