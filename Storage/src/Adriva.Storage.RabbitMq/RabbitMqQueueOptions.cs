namespace Adriva.Storage.RabbitMq
{
    public class RabbitMqQueueOptions
    {
        public string Host { get; set; }

        public int Port { get; set; } = 5672;

        public string Username { get; set; }

        public string Password { get; set; }

        public string VirtualHost { get; set; } = "/";

        public string QueueName { get; set; }

        public string ExchangeName { get; set; }

        public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Direct;

        public bool IsExchangeDurable { get; set; } = true;

        public bool IsQueueDurable { get; set; } = true;

        public string RoutingKey { get; set; }
    }
}