using Adriva.Storage.Abstractions;

namespace Adriva.Storage.RabbitMq
{

    public sealed class RabbitMqQueueClient : RabbitMqClientBase<RabbitMqQueueOptions>
    {
        protected override string GetRoutingKey(QueueMessage message) => base.Options.DefaultRoutingKey;
    }
}
