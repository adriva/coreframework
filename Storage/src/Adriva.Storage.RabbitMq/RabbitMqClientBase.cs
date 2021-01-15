using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Adriva.Common.Core;
using Adriva.Storage.Abstractions;
using Microsoft.Extensions.ObjectPool;
using RabbitMQ.Client;

namespace Adriva.Storage.RabbitMq
{
    internal sealed class ModelPoolPolicy<TOptions> : IPooledObjectPolicy<IModel>, IDisposable where TOptions : RabbitMqQueueOptions, new()
    {
        private readonly TOptions Options;
        private IConnection Connection;

        public ModelPoolPolicy(TOptions options)
        {
            this.Options = options;
        }

        private IConnection GetConnection()
        {
            if (null != this.Connection)
            {
                if (this.Connection.IsOpen) return this.Connection;
                else
                {
                    this.Connection?.Close();
                    this.Connection?.Dispose();
                    this.Connection = null;
                }
            }

            ConnectionFactory connectionFactory = new ConnectionFactory();
            connectionFactory.HostName = this.Options.Host;
            connectionFactory.UserName = this.Options.Username;
            connectionFactory.Password = this.Options.Password;
            connectionFactory.Port = this.Options.Port;
            connectionFactory.VirtualHost = this.Options.VirtualHost;
            this.Connection = connectionFactory.CreateConnection();
            return this.Connection;
        }

        public IModel Create()
        {
            var connection = this.GetConnection();
            var model = connection.CreateModel();

            if (!this.Options.SkipConfigureModel)
            {
                model.ExchangeDeclare(this.Options.ExchangeName, this.Options.ExchangeType, this.Options.IsExchangeDurable, false, null);
                model.QueueDeclare(this.Options.QueueName, this.Options.IsQueueDurable, false, false, null);
                model.QueueBind(this.Options.QueueName, this.Options.ExchangeName, this.Options.DefaultRoutingKey, null);
            }

            return model;
        }

        public bool Return(IModel model)
        {
            if (null == model || !model.IsOpen)
            {
                model?.Close();
                model?.Dispose();
                return false;
            }

            return true;
        }

        public void Dispose()
        {
            this.Connection?.Close();
            this.Connection?.Dispose();
            this.Connection = null;
            GC.SuppressFinalize(this);
        }
    }

    public abstract class RabbitMqClientBase<TOptions> : IQueueClient, IDisposable where TOptions : RabbitMqQueueOptions, new()
    {
        private ObjectPool<IModel> ModelPool;
        private ModelPoolPolicy<TOptions> PoolPolicy;
        private bool IsDisposed;

        protected TOptions Options { get; private set; }

        protected string Name { get; private set; }

        public async ValueTask InitializeAsync(StorageClientContext context)
        {
            this.Options = context.GetOptions<TOptions>();
            this.Name = context.Name;
            this.PoolPolicy = new ModelPoolPolicy<TOptions>(this.Options);
            this.ModelPool = new DefaultObjectPool<IModel>(this.PoolPolicy, Environment.ProcessorCount * 2);
            await Task.CompletedTask;
        }

        protected abstract string GetRoutingKey(QueueMessage message);

        public async ValueTask AddAsync(QueueMessage message, TimeSpan? visibilityTimeout = null, TimeSpan? initialVisibilityDelay = null)
        {
            if (null == message) return;

            await Task.Run(() =>
            {
                string json = Utilities.SafeSerialize(message);
                byte[] buffer = Encoding.UTF8.GetBytes(json);

                var model = this.ModelPool.Get();

                try
                {
                    lock (model)
                    {
                        var properties = model.CreateBasicProperties();
                        properties.ContentType = "application/json";
                        properties.ContentEncoding = "utf-8";
                        properties.MessageId = message.Id;
                        properties.Priority = (byte)(message.Flags & 3L);
                        if (visibilityTimeout.HasValue && 0 < visibilityTimeout.Value.TotalMilliseconds)
                        {
                            properties.Expiration = visibilityTimeout.Value.TotalMilliseconds.ToString();
                        }
                        model.BasicPublish(this.Options.ExchangeName, this.GetRoutingKey(message), true, properties, buffer);
                    }
                }
                finally
                {
                    this.ModelPool.Return(model);
                }

                buffer = null;
            });
        }

        public async Task<QueueMessage> GetNextAsync(CancellationToken cancellationToken)
        {
            return await Task.Run<QueueMessage>(() =>
            {
                var model = this.ModelPool.Get();

                try
                {
                    lock (model)
                    {
                        var result = model.BasicGet(this.Options.QueueName, false);
                        if (null == result) return null;
                        if (0 == result.Body.Length)
                        {
                            model.BasicAck(result.DeliveryTag, false);
                            return null;
                        }

                        string json = Encoding.UTF8.GetString(result.Body.Span);
                        QueueMessage queueMessage = Utilities.SafeDeserialize<QueueMessage>(json);
                        queueMessage.PlatformTag = result.DeliveryTag.ToString();
                        return queueMessage;
                    }
                }
                finally
                {
                    this.ModelPool.Return(model);
                }
            });
        }

        public async Task DeleteAsync(QueueMessage message)
        {
            if (null == message) throw new ArgumentNullException(nameof(message));
            if (!ulong.TryParse(message.PlatformTag, out ulong deliveryTag))
            {
                throw new InvalidOperationException("Invalid platform tag or platform tag not set.");
            }

            await Task.Run(() =>
            {
                var model = this.ModelPool.Get();

                try
                {
                    lock (model)
                    {
                        model.BasicAck(deliveryTag, false);
                    }
                }
                finally
                {
                    this.ModelPool.Return(model);
                }
            });
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.IsDisposed)
            {
                if (disposing)
                {
                    this.PoolPolicy?.Dispose();
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
}
