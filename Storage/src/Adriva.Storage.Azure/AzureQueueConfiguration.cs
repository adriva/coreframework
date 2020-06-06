using System;
using Adriva.Storage.Abstractions;

namespace Adriva.Storage.Azure
{
    public sealed class AzureQueueConfiguration : AzureStorageConfiguration
    {
        internal Type MessageSerializerType { get; private set; } = typeof(DefaultQueueMessageSerializer);

        public string QueueName { get; set; }

        public TimeSpan DefaultTimeToLive { get; set; } = TimeSpan.FromDays(1);

        public void UseSerializer<T>() where T : class, IQueueMessageSerializer
        {
            this.MessageSerializerType = typeof(T);
        }
    }
}
