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

namespace Adriva.Extensions.Analytics.AppInsights
{
#if DEBUG
    public class PersistentChannel : ITelemetryChannel, IDisposable
#else
    internal class PersistentChannel : ITelemetryChannel, IDisposable
#endif
    {
#if DEBUG
        public readonly TelemetryBuffer Buffer;
#else
        private readonly TelemetryBuffer Buffer;
#endif
        private readonly SemaphoreSlim TransmitSemaphore;
        private readonly HttpClient HttpClient;
        private readonly AnalyticsOptions Options;
        private readonly object FileLock = new object();

        private bool IsWorkingFolderReady;
        private Uri EndpointUri;
        private int IsProcessingBackLog;

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

        public PersistentChannel(IOptions<AnalyticsOptions> optionsAccessor)
        {
            this.Options = optionsAccessor.Value;

            int transmitThreadCount = Math.Max(1, this.Options.TransmitThreadCount);
            this.TransmitSemaphore = new SemaphoreSlim(transmitThreadCount, transmitThreadCount);

            this.HttpClient = new HttpClient();
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
                        else
                        {

                            var filePaths = Directory.EnumerateFiles(workingFolder, "*.ai");
                            try
                            {
                                foreach (var filePath in filePaths)
                                {
                                    File.Move(filePath, $"{filePath}.fail");
                                }
                            }
                            catch { }


                        }
                        this.IsWorkingFolderReady = true;
                    }
                }
            }

            return workingFolder;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Potential Code Quality Issues", "RECS0022:A catch clause that catches System.Exception and has an empty body", Justification = "<Pending>")]
        private void OnBufferFull()
        {
            var items = this.Buffer.Dequeue();

            _ = this.TransmitAsync(items);

            if (0 == Interlocked.CompareExchange(ref this.IsProcessingBackLog, 1, 0))
            {
                _ = this.TransmitFailedLogsAsync();
            }
        }

        private async Task TransmitAsync(IEnumerable<ITelemetry> items)
        {
            if (null == items || !items.Any())
            {
                return;
            }

            await this.TransmitSemaphore.WaitAsync();

            byte[] buffer = JsonSerializer.Serialize(items, true);

            string filePath = Path.Combine(this.GetWorkingFolder(), $"{Guid.NewGuid().ToString("N")}.ai");
            try
            {
                using (var fileTransaction = FileTransaction.Create(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
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
            catch
            {
                File.Move(filePath, $"{filePath}.fail");
            }
            finally
            {
                this.TransmitSemaphore.Release();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Potential Code Quality Issues", "RECS0022:A catch clause that catches System.Exception and has an empty body", Justification = "<Pending>")]
        private async Task TransmitFailedLogsAsync()
        {
            string workingFolder = this.GetWorkingFolder();

            var filePaths = Directory.EnumerateFiles(workingFolder, "*.ai.fail");
            try
            {
                foreach (var filePath in filePaths)
                {
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
                Interlocked.Exchange(ref this.IsProcessingBackLog, 0);
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
            this.TransmitSemaphore?.Dispose();
        }
    }
}