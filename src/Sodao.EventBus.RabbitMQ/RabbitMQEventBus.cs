using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Sodao.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sodao.EventBus.RabbitMQ
{
    /// <summary>
    ///  默认的具有重试机制和重连机制的 RabbitMQ 客户端的持久化连接
    /// </summary>
    public class RabbitMQEventBus : IEventBus
    {
        private readonly DiagnosticSource _diagnosticSource;
        private readonly IRabbitMQPersistentConnection _connection;
        /// <summary>
        /// 交换机的名称
        /// </summary>
        private readonly string BROKER_NAME = "sodao_shop_event_bus";
        /// <summary>
        /// 序列化的配置
        /// </summary>
        private readonly JsonSerializerSettings _serializeSetting;
        /// <summary>
        /// 发布消息的重试次数
        /// </summary>
        private readonly int _retryCount;
        /// <summary>
        /// 用于分发事件消息的持久化通道
        /// </summary>
        private IModel _channel;
        /// <summary>
        /// 顶级的ioc容器
        /// </summary>
        private readonly IServiceProvider _service;
        /// <summary>
        /// 当前应用所使用的消息队列名称
        /// </summary>
        private readonly string _queueName;
        private readonly IDictionary<string, ISet<Type>> _eventTypeMap;
        private readonly ILogger _logger;
        /// <summary>
        /// 初始化 <see cref="RabbitMQEventBus"/> 类型的新实例。
        /// </summary>
        /// <param name="connection">用于连接 RabbitMQ 的持久化连接 </param>
        /// <param name="logger">用于记录事件总线行为的日志器</param>
        /// <param name="options">用于 RabbitMQEventBus 的配置</param>
        /// <param name="serviceProvider">用于创建 IEventHandler 对象的顶级IOC容器</param>
        /// <param name="diagnosticSource">用于遥测该事件总线</param>
        public RabbitMQEventBus(IRabbitMQPersistentConnection connection,
            ILogger<RabbitMQEventBus> logger,
            IOptions<RabbitMQEventBusOptions> options,
            IServiceProvider serviceProvider,
            DiagnosticSource diagnosticSource)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            BROKER_NAME = options.Value.BrokerName;

            _serializeSetting = new JsonSerializerSettings
            {
            };
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _retryCount = options.Value.PublishRetryCount;
            _eventTypeMap = new Dictionary<string, ISet<Type>>();
            _queueName = options.Value.QueueName ?? throw new InvalidOperationException("No queue name are specified in RabbitMQEventBusOptions");
            _service = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _diagnosticSource = diagnosticSource ?? throw new ArgumentNullException(nameof(diagnosticSource));
            _channel = CreateConsumer();

        }
        /// <inheritdoc />
        public void Publish<TEvent>(TEvent @event)
            where TEvent : IntegrationEvent
        {
            if (@event == null)
            {
                throw new ArgumentNullException(nameof(@event));
            }

            Connect();

            using (var channel = _connection.CreateModel())
            {
                var eventName = @event.GetType().Name;

                channel.ExchangeDeclare(BROKER_NAME, ExchangeType.Direct);

                var message = JsonConvert.SerializeObject(@event, typeof(TEvent), _serializeSetting);

                var body = Encoding.UTF8.GetBytes(message);

                var success = false;
                for (var time = 0; time < _retryCount; time++)
                {
                    try
                    {
                        _diagnosticSource.BeforePublushEvent(@event);

                        channel.BasicPublish(exchange: BROKER_NAME,
                            routingKey: eventName,
                            basicProperties: null,
                            body: body);

                        _diagnosticSource.AfterPublushEvent(@event);
                        success = true;
                        break;
                    }
                    catch (BrokerUnreachableException ex)
                    {
                        _logger.LogWarning("Rabbit Client publish message {3}： {0}，{1} fail", @event.Id, @event.ToString(), @event.GetType());
                        _logger.LogWarning(ex.ToString());
                        _diagnosticSource.PublishRetry(@event, ex, time);
                    }
                    catch (SocketException ex)
                    {
                        _logger.LogWarning("Rabbit Client publish message {3}： {0}，{1} fail", @event.Id, @event.ToString(), @event.GetType());
                        _logger.LogWarning(ex.ToString());
                        _diagnosticSource.PublishRetry(@event, ex, time);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogCritical(ex, "FALTAL ERROR:A exception was occur during Rabbit Client publish message..");
                        _diagnosticSource.PublishFail(@event, ex);
                        throw ex;
                    }
                    finally
                    {
                        time++;
                    }
                }

                if (!success)
                {
                    _diagnosticSource.PublishRetryFailed(@event);
                }
            }

        }

        /// <inheritdoc />
        Task IEventBus.PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken)
        {
            return Task.Run(() => Publish(@event), cancellationToken);
        }

        /// <inheritdoc />
        void IEventBus.Subscribe<TEvent, TEventHandler>()
        {
            var eventName = typeof(TEvent).Name;
            if (!_eventTypeMap.TryGetValue(eventName, out var handlers))
            {
                handlers = new HashSet<Type>();
                _eventTypeMap.Add(eventName, handlers);
            }

            if (!handlers.Add(typeof(TEventHandler)))
            {
                throw new InvalidOperationException($"事件处理程序 {typeof(TEventHandler)} 已注册，无法重复注册");
            }

            Connect();
            using (var channel = _connection.CreateModel())
            {
                channel.QueueBind(_queueName, BROKER_NAME, eventName);
            }
        }

        /// <inheritdoc />
        void IEventBus.Unsubscribe<TEvent, TEventHandler>()
        {
            var eventName = typeof(TEvent).Name;
            if (!_eventTypeMap.ContainsKey(eventName))
            {
                return;
            }
            var handlers = _eventTypeMap[eventName];

            handlers.Remove(typeof(TEventHandler));

            if (!handlers.Any())
            {
                _eventTypeMap.Remove(eventName);
                Connect();
                using (var channel = _connection.CreateModel())
                {
                    channel.QueueUnbind(_queueName, BROKER_NAME, eventName);
                }
            }
        }


        /// <summary>
        /// Create a default consumer that don't accept any event message.
        /// </summary>
        /// <returns></returns>
        private IModel CreateConsumer()
        {
            Connect();

            var channel = _connection.CreateModel();

            channel.ExchangeDeclare(exchange: BROKER_NAME,
                type: ExchangeType.Direct);

            channel.QueueDeclare(queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += Consumer_Received;

            channel.BasicConsume(_queueName, false, consumer);

            channel.CallbackException += Channel_CallbackException;

            return channel;
        }
        private async void Consumer_Received(object sender, BasicDeliverEventArgs e)
        {
            _diagnosticSource.BeforeReceived(sender, e);

            var eventName = e.RoutingKey;

            var message = Encoding.UTF8.GetString(e.Body);
            try
            {
                await ProcessEventAsync(eventName, message);
            }
            catch(RabbitMQEventBusUnInitializedException error)
            {
                _logger.LogWarning(error,"will retry 1 seconds later");

                // 等待一秒钟后拒绝事件，进行重试
                await Task.Delay(1000);
                _channel.BasicNack(e.DeliveryTag, false, true);

                return;
            }
            catch (Exception error)
            {
                _logger.LogError(error, "A error was occur during handing event,please check the consumer state");

                await Task.Delay(1000);

                _channel.BasicNack(e.DeliveryTag, false, true);
                return;
            }
            _channel.BasicAck(e.DeliveryTag, false);

            _diagnosticSource.AfterReceived(e);
        }
        private async Task ProcessEventAsync(string eventName, string jsonMessage)
        {
            var hasHandlers = _eventTypeMap.TryGetValue(eventName, out var handlers);

            Debug.Assert(hasHandlers);
            if (!hasHandlers)
            {
                // current rabbit mq has started,but handlers were not subscribed
                throw new RabbitMQEventBusUnInitializedException(eventName);
            }
            using (var scope = _service.CreateScope())
            {
                foreach (var t in handlers)
                {
                    var instance = ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, t);

                    var methodInfo = instance.GetType()
                        .GetMethod("HandleAsync");

                    var eventType = t.GetInterfaces()
                        .Where(x => x.IsGenericType)
                        .Single()
                        .GetGenericArguments()[0];

                    var @event = JsonConvert.DeserializeObject(jsonMessage, eventType, _serializeSetting);

                    var task = (Task)methodInfo.Invoke(instance, new[] { @event, CancellationToken.None });
                    await task;


                }
            }
        }
        private void Channel_CallbackException(object sender, CallbackExceptionEventArgs e)
        {
            _channel.Dispose();
            _channel = CreateConsumer();
        }

        #region 连接RabbitMQ消息队列
        /// <summary>
        /// 连接RabbitMQ消息队列
        /// </summary>
        private void Connect()
        {
            if (!_connection.IsConnected)
            {
                if (!_connection.TryConnect())
                {
                    throw new InvalidOperationException("RabbitMQ connection could not be created and opened");
                }
            }
        }
        #endregion
    }
}
