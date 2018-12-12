using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Rabble.EventBus.RabbitMQ
{
    /// <summary>
    /// 默认的具有重试机制和重连机制的 RabbitMQ 客户端的持久化连接
    /// </summary>
    public class RabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {
        private readonly ILogger _logger;
        private IConnection _connection;
        private readonly int _retryCount;
        private readonly object _sync;
        private readonly IConnectionFactory _factory;
        /// <inheritdoc />
        public bool IsConnected => _connection != null && _connection.IsOpen && !disposedValue;
        /// <inheritdoc />
        public RabbitMQPersistentConnection(ILogger<RabbitMQPersistentConnection> logger,
           IOptions<RabbitMQEventBusOptions> options,
           IConnectionFactory factory)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _retryCount = options.Value.ConnectRetryCount;
            _sync = new object();
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }
        IModel IRabbitMQPersistentConnection.CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return _connection.CreateModel();
        }
        /// <summary>
        /// 尝试连接
        /// </summary>
        /// <returns>如果连接成功，返回true，否则返回false</returns>
        public bool TryConnect()
        {
            _logger.LogInformation("RabbitMQ Client is trying to connect...");

            lock (_sync)
            {
                var time = 0;
                while (!IsConnected && time < _retryCount)
                {
                    try
                    {
                        _connection = _factory.CreateConnection();
                    }
                    catch (BrokerUnreachableException ex)
                    {
                        _logger.LogWarning("RabbitMQ Client has connect fail {0} times", time);
                        _logger.LogWarning(ex.ToString());
                    }
                    catch (SocketException ex)
                    {
                        _logger.LogWarning("RabbitMQ Client has connect fail {0} times", time);
                        _logger.LogWarning(ex.ToString());
                    }
                    finally
                    {
                        time++;
                    }
                }
                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    _logger.LogInformation($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");
                    return true;
                }
                else
                {
                    _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
                    return false;
                }
            }
        }
        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (disposedValue) return;

            _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

            TryConnect();
        }

        private void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (disposedValue) return;

            _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

            TryConnect();
        }

        private void OnConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            if (disposedValue) return;

            _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

            TryConnect();
        }
        #region IDisposable Support
        private bool disposedValue = false; // 要检测冗余调用

       
        /// <summary>
        /// 释放持久化的长连接
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _connection.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。
                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~RabbitMQPersistentConnection() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        void IDisposable.Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing) 中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
