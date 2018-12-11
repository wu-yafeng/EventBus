using Sodao.EventBus.Abstractions;
using System;
using System.Linq;
using System.Reflection;

namespace Sodao.EventBus.RabbitMQ
{
    /// <summary>
    /// 消息队列扩展
    /// </summary>
    public static class EventBusExtensions
    {
        private static readonly MethodInfo _subscribeGenericMethod = typeof(IEventBus).GetMethods()
            .Where(m => m.IsGenericMethod && m.Name == nameof(IEventBus.Subscribe))
            .Single();
        /// <summary>
        /// subscribe all handlers defined in assembly
        /// </summary>
        /// <param name="eventbus"></param>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IEventBus Subscribe(this IEventBus eventbus, params Assembly[] assemblies)
        {
            var handlerTypes = assemblies.Where(a => a != null)
                .SelectMany(a => a.GetTypes())
                .Where(IsEventHandler)
                .ToArray();

            return Subscribe(eventbus, handlerTypes);
        }


        /// <summary>
        /// subscribe all handlers
        /// </summary>
        /// <param name="eventbus"></param>
        /// <param name="handlerTypes"></param>
        /// <returns></returns>
        public static IEventBus Subscribe(this IEventBus eventbus, params Type[] handlerTypes)
        {
            foreach (var handler in handlerTypes.Where(IsEventHandler))
            {
                var eventType = handler
                    .GetInterfaces()
                    .Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                    .GetGenericArguments()[0];

                Subscribe(eventbus, eventType, handler);
            }

            return eventbus;
        }

        private static bool IsEventHandler(this Type type)
        {
            return type != null
                && !type.IsAbstract
                && !type.IsInterface
                && type.GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEventHandler<>));
        }

        private static void Subscribe(IEventBus instance, Type eventType, Type handlerType)
        {
            _subscribeGenericMethod.MakeGenericMethod(eventType, handlerType).Invoke(instance, null);
        }
    }
}
