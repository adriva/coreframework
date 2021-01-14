using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;
using RabbitMQ.Client;

namespace Adriva.Storage.RabbitMq
{

    public sealed class RabbitMqQueueClient : IQueueClient, IDisposable
    {
        private object OperationLock = new object();
        private RabbitMqQueueOptions Options;
        private ConnectionFactory ConnectionFactory;
        private IConnection Connection;
        private IModel Model;

        private string QueueName;
        private string ExchangeName;
        private string RoutingKey => $"{this.QueueName}_RK";

        public ValueTask InitializeAsync(StorageClientContext context)
        {
            this.Options = context.GetOptions<RabbitMqQueueOptions>();

            this.QueueName = $"{context.Name}_Q";
            this.ExchangeName = $"{context.Name}_EXC";

            this.ConnectionFactory = new ConnectionFactory();
            this.ConnectionFactory.Port = this.Options.Port;
            this.ConnectionFactory.UserName = this.Options.Username;
            this.ConnectionFactory.Password = this.Options.Password;
            this.ConnectionFactory.HostName = this.Options.Host;
            this.ConnectionFactory.AutomaticRecoveryEnabled = true;

            return new ValueTask();
        }

        private IModel GetModel()
        {
            if (null != this.Model)
            {
                if (!this.Model.IsOpen)
                {
                    this.Model.Dispose();
                    this.Model = null;
                }
                else return this.Model;
            }

            if (null != this.Connection)
            {
                if (!this.Connection.IsOpen)
                {
                    this.Connection.Dispose();
                    this.Connection = null;
                }
            }

            if (null == this.Connection)
            {
                this.Connection = this.ConnectionFactory.CreateConnection();
            }

            if (null == this.Model)
            {
                this.Model = this.Connection.CreateModel();
                this.Model.ExchangeDeclare(this.ExchangeName, ExchangeType.Direct, true, false, null);
                this.Model.QueueDeclare(this.QueueName, true, false, false, null);
                this.Model.QueueBind(this.QueueName, this.ExchangeName, this.RoutingKey, null);
            }

            return this.Model;
        }

        public async ValueTask AddAsync(QueueMessage message, TimeSpan? visibilityTimeout = null, TimeSpan? initialVisibilityDelay = null)
        {
            if (null == message) return;

            await Task.Run(() =>
            {
                string json = Utilities.SafeSerialize(message);
                byte[] buffer = Encoding.UTF8.GetBytes(json);

                lock (this.OperationLock)
                {
                    var model = this.GetModel();
                    var properties = model.CreateBasicProperties();
                    properties.ContentType = "application/json";
                    properties.ContentEncoding = "utf-8";
                    properties.MessageId = message.Id;
                    properties.Priority = (byte)(message.Flags & 3L);
                    if (visibilityTimeout.HasValue && 0 < visibilityTimeout.Value.TotalMilliseconds)
                    {
                        properties.Expiration = visibilityTimeout.Value.TotalMilliseconds.ToString();
                    }
                    model.BasicPublish(this.ExchangeName, this.RoutingKey, true, properties, buffer);
                }

                buffer = null;
            });
        }

        public async Task<QueueMessage> GetNextAsync(CancellationToken cancellationToken)
        {
            return await Task.Run<QueueMessage>(() =>
            {
                lock (this.OperationLock)
                {
                    var model = this.GetModel();
                    var result = model.BasicGet(this.QueueName, false);

                    if (null == result) return null;
                    string json = Encoding.UTF8.GetString(result.Body.Span);
                    QueueMessage queueMessage = Utilities.SafeDeserialize<QueueMessage>(json);
                    queueMessage.PlatformTag = Convert.ToString(result.DeliveryTag);
                    return queueMessage;
                }
            });
        }

        public async Task DeleteAsync(QueueMessage message)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));

            if (!ulong.TryParse(message.PlatformTag, out ulong deliveryTag))
            {
                throw new InvalidOperationException("Message doesnt have or has an invalid paltform tag.");
            }

            await Task.Run(() =>
            {
                var model = this.GetModel();
                model.BasicAck(deliveryTag, false);
            });
        }

        public void Dispose()
        {
            this.Model?.Close();
            this.Model?.Dispose();

            this.Connection?.Close();
            this.Connection?.Dispose();

            this.Model = null;
            this.Connection = null;

            GC.SuppressFinalize(this);
        }
    }
}
