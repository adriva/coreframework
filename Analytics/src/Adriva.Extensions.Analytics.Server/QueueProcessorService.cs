using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Adriva.Extensions.Analytics.Server
{
    public class QueueProcessorService : IHostedService, IDisposable
    {
        private readonly IQueueingService QueueingService;
        private readonly AutoResetEvent ProcessingCompleteSignal = new AutoResetEvent(false);
        private bool IsDisposed;

        public QueueProcessorService(IHostApplicationLifetime applicationLifetime, IQueueingService queueingService)
        {
            this.QueueingService = queueingService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _ = Task.Run(async () => { await this.ProcessItemsAsync(); });
            await Task.CompletedTask;
        }

        private async Task ProcessItemsAsync()
        {
            var enumerable = this.QueueingService.GetConsumingEnumerable();
            using (var enumerator = enumerable.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    await Task.CompletedTask;
                }
            }

            this.ProcessingCompleteSignal.Set();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            this.ProcessingCompleteSignal.WaitOne();
            return Task.CompletedTask;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.ProcessingCompleteSignal.Dispose();
                }

                this.IsDisposed = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~QueueProcessorService()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
