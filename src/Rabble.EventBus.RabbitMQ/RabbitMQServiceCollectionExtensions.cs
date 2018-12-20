using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Rabble.EventBus.Abstractions;
using Rabble.EventBus.RabbitMQ;
using System;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// 
    /// </summary>
    public static class RabbitMQServiceCollectionExtensions
    {
        /// <summary>
        /// 添加RabbitMQ实现的事件总线
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public static IServiceCollection AddRabbitMQEventBus(this IServiceCollection services, Action<RabbitMQEventBusOptions> configure)
        {
            services.Configure(configure);

            services.TryAddSingleton<IEventBus>(sp =>
            {
                var connection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                var logger = sp.GetRequiredService<ILogger<RabbitMQEventBus>>();
                var options = sp.GetRequiredService<IOptions<RabbitMQEventBusOptions>>();
                var diagnosticSource = sp.GetRequiredService<DiagnosticSource>();
                return new RabbitMQEventBus(connection, logger, options, sp.CreateScope().ServiceProvider, diagnosticSource);
            });

            services.TryAddSingleton<IRabbitMQPersistentConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<RabbitMQPersistentConnection>>();
                var options = sp.GetRequiredService<IOptions<RabbitMQEventBusOptions>>();
                var connectionFactory = new ConnectionFactory()
                {
                    HostName = options.Value.Host,
                    Password = options.Value.Password,
                    UserName = options.Value.UserName,
                    Port = options.Value.Port
                };
                return new RabbitMQPersistentConnection(logger, options, connectionFactory);
            });

            // if the environment is aspnetcore web api,the diagnostic are supported.
            if (!services.Any(x => x.ServiceType == typeof(DiagnosticSource)))
            {
                var listener = new DiagnosticListener("Microsoft.AspNetCore");
                services.TryAddSingleton<DiagnosticSource>(listener);
            }
            return services;
        }

#if NET451
        internal static IServiceScope CreateScope(this IServiceProvider provider)
            => provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
#endif
    }
}