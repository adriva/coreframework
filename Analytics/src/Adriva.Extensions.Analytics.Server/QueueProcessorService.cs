using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Extensions.Analytics.Server.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Adriva.Extensions.Analytics.Server
{
    /// <summary>
    /// Provides the queueing (buffering) services required to store parsed analytics data temporarily until it's sent to the repository.
    /// </summary>
    internal class QueueProcessorService : IHostedService, IDisposable
    {
        private readonly AnalyticsServerOptions Options;
        private readonly IAnalyticsRepository Repository;
        private readonly IQueueingService QueueingService;
        private readonly CancellationTokenSource StopTokenSource = new CancellationTokenSource();
        private readonly Task[] ProcessorTasks;
        private bool IsDisposed;

        public QueueProcessorService(IQueueingService queueingService, IAnalyticsRepository repository, IOptions<AnalyticsServerOptions> optionsAccessor)
        {
            this.Options = optionsAccessor.Value;
            this.Repository = repository;
            this.QueueingService = queueingService;

            this.ProcessorTasks = new Task[this.Options.ProcessorThreadCount];
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            for (int loop = 0; loop < this.Options.ProcessorThreadCount; loop++)
            {
                this.ProcessorTasks[loop] = Task.Run(async () => { await this.ProcessItemsAsync(); });
            }

            return Task.CompletedTask;
        }

        private async Task ProcessItemsAsync()
        {
            List<AnalyticsItem> buffer = new List<AnalyticsItem>();
            var enumerable = this.QueueingService.GetConsumingEnumerable(this.StopTokenSource.Token);
            using (var enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    buffer.Add(enumerator.Current);

                    if (this.Options.BufferCapacity <= buffer.Count)
                    {
                        try
                        {
                            using (CancellationTokenSource storeOperationCancellationTokenSource = new CancellationTokenSource())
                            {
                                storeOperationCancellationTokenSource.CancelAfter(this.Options.StorageTimeout);
                                await this.Repository.StoreAsync(buffer, storeOperationCancellationTokenSource.Token);
                            }
                            buffer.Clear();
                        }
                        catch (Exception exception)
                        {
                            try
                            {
                                await this.Repository.HandleErrorAsync(buffer, exception);
                            }
                            catch
                            {
                                // HandleErrorAsync method cannot throw exceptions
                            }
                            finally
                            {
                                buffer.Clear();
                            }
                        }
                    }
                }
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
