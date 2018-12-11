using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sodao.EventBus.RabbitMQ
{
    /// <summary>
    /// 表示 RabbitMQ 客户端连接服务端的持久化连接对象
    /// </summary>
    public interface IRabbitMQPersistentConnection : IDisposable
    {
        /// <summary>
        /// 尝试连接 RabbitMQ 消息队列
        /// </summary>
        /// <returns>如果连接成功，返回 <see langword="true"/> ，否则，返回 <see langword="false"/></returns>
        bool TryConnect();
        /// <summary>
        /// 获取一个值，该值指示 RabbitMQ 客户端是否已经连接到消息队列
        /// </summary>
        bool IsConnected { get; }
        /// <summary>
        /// 创建一个用于和 RabbitMQ 消息队列通讯的通道
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">未连接RabbitMQ服务器</exception>
        IModel CreateModel();
    }
}
