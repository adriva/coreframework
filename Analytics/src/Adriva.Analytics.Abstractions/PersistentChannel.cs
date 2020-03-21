using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Extensions.Options;

namespace Adriva.Analytics.Abstractions
{
#if DEBUG
    public class PersistentChannel : ITelemetryChannel, IDisposable
#else
    internal class PersistentChannel : ITelemetryChannel, IDisposable
#endif
    {
        private readonly TelemetryBuffer Buffer;
        private readonly ManualResetEvent FileSignal = new ManualResetEvent(true);
        private readonly HttpClient HttpClient;
        private readonly AnalyticsOptions Options;
        private readonly object FileLock = new object();

        private int IsProcessingBackLog = 0;
        private bool IsWorkingFolderReady;
        private Uri EndpointUri;

#if DEBUG
        public TelemetryBuffer TelemetryBuffer => this.Buffer;
#endif

        public bool? DeveloperMode { get; set; }

        public string EndpointAddress
        {
            get => this.EndpointUri?.ToString();
            set
            {
                if (!Uri.TryCreate(value, UriKind.Absolute, out Uri uri))
                {
                    this.EndpointUri = null;
                    throw new UriFormatException("Invalid endpoint Uri specified.");
                }

                this.EndpointUri = uri;
            }
        }

        public string LocalFolder { get; set; }

        public PersistentChannel(IHttpClientFactory httpClientFactory, IOptions<AnalyticsOptions> optionsAccessor)
        {
            this.HttpClient = httpClientFactory.CreateClient();
            this.Options = optionsAccessor.Value;

            this.LocalFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            this.Buffer = new TelemetryBuffer();
            this.Buffer.OnFull = this.OnBufferFull;
            this.Buffer.BacklogSize = this.Options.BacklogSize;
            this.Buffer.Capacity = this.Options.Capacity;
        }

        private string GetWorkingFolder()
        {
            string workingFolder = Path.Combine(this.LocalFolder, "analytics");

            if (!this.IsWorkingFolderReady)
            {
                lock (this.FileLock)
                {
                    if (!this.IsWorkingFolderReady)
                    {
                        if (!Directory.Exists(workingFolder))
                        {
                            Directory.CreateDirectory(workingFolder);
                        }

                        this.IsWorkingFolderReady = true;
                    }
                }
            }

            return workingFolder;
        }

        private FileTransaction GetLogFileTransaction()
        {
            string filePath = Path.Combine(this.GetWorkingFolder(), $"{Guid.NewGuid().ToString("N")}.ai");
            return FileTransaction.Create(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Potential Code Quality Issues", "RECS0022:A catch clause that catches System.Exception and has an empty body", Justification = "<Pending>")]
        private void OnBufferFull()
        {
            var items = this.Buffer.Dequeue();

            _ = this.TransmitAsync(items);
            _ = this.TransmitFailedLogsAsync();
        }

        private async Task TransmitAsync(IEnumerable<ITelemetry> items)
        {
            if (null == items || !items.Any())
            {
                return;
            }

            this.FileSignal.WaitOne();

            byte[] buffer = JsonSerializer.Serialize(items, true);

            try
            {

                using (var fileTransaction = this.GetLogFileTransaction())
                {
                    await fileTransaction.WriteAsync(buffer);
                    using (var content = new ByteArrayContent(buffer))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue(JsonSerializer.ContentType);
                        await this.HttpClient.PostAsync(this.EndpointUri.ToString(), content);
                    }
                    fileTransaction.Commit();
                }
            }
            finally
            {
                this.FileSignal.Set();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Potential Code Quality Issues", "RECS0022:A catch clause that catches System.Exception and has an empty body", Justification = "<Pending>")]
        private async Task TransmitFailedLogsAsync()
        {
            if (0 != Interlocked.CompareExchange(ref this.IsProcessingBackLog, 1, 0)) return;

            if (!this.FileSignal.WaitOne(1000)) return;

            string workingFolder = this.GetWorkingFolder();

            var filePaths = Directory.EnumerateFiles(workingFolder, "*.ai");
            try
            {
                foreach (var filePath in filePaths)
                {
                    string name = Path.GetFileNameWithoutExtension(filePath);

                    if (!Guid.TryParse(name, out _)) continue;
                    try
                    {
                        using (var transaction = FileTransaction.Create(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            byte[] buffer = File.ReadAllBytes(filePath);
                            using (var content = new ByteArrayContent(buffer))
                            {
                                content.Headers.ContentType = new MediaTypeHeaderValue(JsonSerializer.ContentType);
                                await this.HttpClient.PostAsync(this.EndpointUri.ToString(), content);
                            }
                            transaction.Commit();
                        }
                    }
                    catch { }
                }
            }
            finally
            {
                Interlocked.Exchange(ref this.IsProcessingBackLog, 1);
                this.FileSignal.Set();
            }
        }

        public void Send(ITelemetry item)
        {
            this.Buffer.Enqueue(item);
        }

        public void Flush()
        {
            this.OnBufferFull();
        }

        public void Dispose()
        {
            this.FileSignal?.Dispose();
        }
    }
}