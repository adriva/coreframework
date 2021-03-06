using System;
using Adriva.Storage.RabbitMq;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class RabbitMqStorageExtensions
    {
        public static IStorageBuilder AddRabbitMqQueue(this IStorageBuilder builder, string name, Action<RabbitMqQueueOptions> configure)
        {
            builder.AddQueueClient<RabbitMqQueueClient, RabbitMqQueueOptions>(name, configure);
            return builder;
        }
    }
}