using Microsoft.Extensions.DependencyInjection.Extensions;
using Rabble.EventBus.Abstractions;
using Rabble.EventBus.InMemory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class InMemoryEventBusServiceCollectionExtensions
    {
        /// <summary>
        /// 添加RabbitMQ实现的事件总线
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddInMemoryEventBus(this IServiceCollection services)
        {

            services.TryAddSingleton<IEventBus>(sp =>
            {
                return new InMemoryEventBus(sp.CreateScope().ServiceProvider);
            });
            return services;
        }
    }
}
