using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Extensions.Analytics.Server.Entities;
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
        private readonly IAnalyticsRepository Repository;
        private readonly IQueueingService QueueingService;
        private readonly CancellationTokenSource StopTokenSource = new CancellationTokenSource();
        private readonly Task[] ProcessorTasks;
        private bool IsDisposed;

        public QueueProcessorService(IQueueingService queueingService,
                                    IAnalyticsRepository repository,
                                    IOptions<AnalyticsServerOptions> optionsAccessor,
                                    ILogger<QueueProcessorService> logger)
        {
            this.Options = optionsAccessor.Value;
            this.Repository = repository;
            this.QueueingService = queueingService;
            this.Logger = logger;

            this.ProcessorTasks = new Task[this.Options.ProcessorThreadCount];
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            for (int loop = 0; loop < this.Options.ProcessorThreadCount; loop++)
            {
                this.Logger.LogInformation($"Starting queue processor instance {loop}.");
                this.ProcessorTasks[loop] = Task.Run(async () => { await this.ProcessItemsAsync(); });
            }

            return Task.CompletedTask;
        }

        private async Task PersistBufferAsync(List<AnalyticsItem> buffer)
        {
            if (0 == buffer.Count) return;

            try
            {
                await this.Repository.StoreAsync(buffer, CancellationToken.None);
                this.Logger.LogTrace($"AnalyticsItems persisted in repository.");
            }
            catch (Exception storageError)
            {
                try
                {
                    await this.Repository.HandleErrorAsync(buffer, storageError);
                }
                catch (Exception errorHandlerError)
                {
                    this.Logger.LogError(errorHandlerError, $"Repository '{this.Repository.GetType().FullName}' failed to handle error.");
                }
            }
            finally
            {
                buffer.Clear();
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
