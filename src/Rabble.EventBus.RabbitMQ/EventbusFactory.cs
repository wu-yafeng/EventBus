#if NET451
using Microsoft.Extensions.DependencyInjection;
using Rabble.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rabble.EventBus.RabbitMQ
{
    /// <summary>
    /// A Factory for creating <see cref="IEventBus"/>
    /// </summary>
    public static class EventbusFactory
    {
        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initialize(Action<RabbitMQEventBusOptions> configure)
        {
            if (_sp != null)
            {
                return;
            }
            _sp = new ServiceCollection()
                .AddRabbitMQEventBus(configure)
                .BuildServiceProvider();
        }
        private static IServiceProvider _sp;
        /// <summary>
        /// IOC容器
        /// </summary>
        public static IServiceProvider ServiceProvider => _sp;
    }
}

#endif