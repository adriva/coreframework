using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Extensions.Analytics.Server.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Analytics.Server
{
    /// <summary>
    /// Provides the queueing (buffering) services required to store parsed analytics data temporarily until it's sent to the repository.
    /// </summary>
    internal class QueueProcessorService : IHostedService, IDisposable
    {
        private readonly ILogger Logger;
        private readonly AnalyticsServerOptions Options;
        private readonly IServiceProvider ServiceProvider;
        private readonly IQueueingService QueueingService;
        private readonly CancellationTokenSource StopTokenSource = new CancellationTokenSource();
        private readonly Task[] ProcessorTasks;
        private bool IsDisposed;

        public QueueProcessorService(
                                    IServiceProvider serviceProvider,
                                    IQueueingService queueingService,
                                    IOptions<AnalyticsServerOptions> optionsAccessor,
                                    ILogger<QueueProcessorService> logger)
        {
            this.ServiceProvider = serviceProvider;
            this.Options = optionsAccessor.Value;
            this.QueueingService = queueingService;
            this.Logger = logger;

            this.ProcessorTasks = new Task[this.Options.ProcessorThreadCount];
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (IServiceScope serviceScope = this.ServiceProvider.CreateScope())
            {
                IAnalyticsRepository repository = serviceScope.ServiceProvider.GetRequiredService<IAnalyticsRepository>();
                await repository.InitializeAsync();
            }

            for (int loop = 0; loop < this.Options.ProcessorThreadCount; loop++)
            {
                this.Logger.LogInformation($"Starting queue processor instance {loop}.");
                this.ProcessorTasks[loop] = Task.Run(async () => { await this.ProcessItemsAsync(); });
            }
        }

        private async Task PersistBufferAsync(List<AnalyticsItem> buffer)
        {
            if (0 == buffer.Count) return;


            using (IServiceScope serviceScope = this.ServiceProvider.CreateScope())
            {
                IAnalyticsRepository repository = serviceScope.ServiceProvider.GetRequiredService<IAnalyticsRepository>();

                try
                {
                    await repository.StoreAsync(buffer, CancellationToken.None);
                    this.Logger.LogTrace($"AnalyticsItems persisted in repository.");
                }
                catch (Exception storageError)
                {
                    try
                    {
                        await repository?.HandleErrorAsync(buffer, storageError);
                    }
                    catch (Exception errorHandlerError)
                    {
                        this.Logger.LogError(errorHandlerError, $"Repository '{repository.GetType().FullName}' failed to handle error.");
                    }
                }
                finally
                {
                    buffer.Clear();
                }
            }
        }

        private async Task ProcessItemsAsync()
        {
            while (!this.QueueingService.IsCompleted)
            {
                List<AnalyticsItem> buffer = new List<AnalyticsItem>(1 + this.Options.BufferCapacity);

                while (this.QueueingService.TryGetNext(10000, out AnalyticsItem analyticsItem))
                {
                    buffer.Add(analyticsItem);

                    if (this.Options.BufferCapacity <= buffer.Count)
                    {
                        await this.PersistBufferAsync(buffer);
                    }
                }

                await this.PersistBufferAsync(buffer);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.StopTokenSource.Cancel();
            Task.WaitAll(this.ProcessorTasks);
            return Task.CompletedTask;
        }

        #region IDisposable Implementation

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.StopTokenSource.Dispose();
                }

                this.IsDisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
    #endregion
}
