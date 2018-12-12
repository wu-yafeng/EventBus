using System;
using System.Collections.Generic;
using System.Text;

namespace Rabble.EventBus.RabbitMQ
{
    /// <summary>
    /// RabbitMQ 事件总线的配置
    /// </summary>
    public class RabbitMQEventBusOptions
    {
        /// <summary>
        /// 获取或设置一个值，该值指示发布消息到事件总线失败时，进行重试的次数。默认值为 5
        /// </summary>
        /// <exception cref="InvalidOperationException">次数不能小于1</exception>
        public int PublishRetryCount
        {
            get => _publishRetryCount;
            set => _publishRetryCount = value < 1
                ? throw new InvalidOperationException("the value must be greater than zero")
                : value;
        }
        private int _publishRetryCount = 5;
        /// <summary>
        /// 获取或设置一个值，该值指示连接RabbitMQ消息队列出现网络状态时，进行重新连接的次数。默认值为 5
        /// </summary>
        /// <exception cref="InvalidOperationException">次数不能小于1</exception>
        public int ConnectRetryCount
        {
            get => _connectRetryCount;
            set => _connectRetryCount = value < 1
                ? throw new InvalidOperationException("the value must be greater than zero")
                : value;
        }
        private int _connectRetryCount = 5;
        /// <summary>
        /// 获取或设置一个值，该值指示连接 RabbitMQ 的 Host 地址。 默认为 localhost
        /// </summary>
        public string Host { get; set; } = "localhost";
        /// <summary>
        /// 获取或设置一个值，该值指示连接 RabbitMQ 的用户名。默认为 guest.
        /// </summary>
        public string UserName { get; set; } = "guest";
        /// <summary>
        /// 获取或设置一个值，该值指示连接 RabbitMQ 的密码。默认为 guest.
        /// </summary>
        public string Password { get; set; } = "guest";
        /// <summary>
        /// 当前的应用的队列实例名称
        /// </summary>
        public string QueueName { get; set; } = AppDomain.CurrentDomain.FriendlyName;
        /// <summary>
        /// 获取或设置一个值，该值指示RabbitMQ的连接端口
        /// </summary>
        public int Port { get; set; } = 5672;
        /// <summary>
        /// 交换机名称 default is 'Rabble_shop_event_bus'
        /// </summary>
        public string BrokerName { get; set; } = "ha_rabble_event_bus";
    }
}
