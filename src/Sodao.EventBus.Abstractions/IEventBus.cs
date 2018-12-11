using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sodao.EventBus.Abstractions
{
    /// <summary>
    /// 用于承载发布事件、订阅事件和分发事件到订阅者的事件总线
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// 异步的方式发布一个事件到该事件总线
        /// </summary>
        /// <typeparam name="TEvent">事件的类型</typeparam>
        /// <param name="event">事件的数据</param>
        /// <param name="cancellationToken">用于取消发布任务的 token</param>
        /// <returns></returns>
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default(CancellationToken))
            where TEvent : IntegrationEvent;
        /// <summary>
        /// 同步的方式发布一个事件到该事件总线
        /// </summary>
        /// <typeparam name="TEvent">事件的类型</typeparam>
        /// <param name="event">事件的数据</param>
        void Publish<TEvent>(TEvent @event)
            where TEvent : IntegrationEvent;
        /// <summary>
        /// 订阅指定的事件
        /// </summary>
        /// <typeparam name="TEvent">事件的类型</typeparam>
        /// <typeparam name="TEventHandler">事件的处理程序类型或消费者的类型</typeparam>
        void Subscribe<TEvent, TEventHandler>()
            where TEvent : IntegrationEvent
            where TEventHandler : IEventHandler<TEvent>;
        /// <summary>
        /// 取消订阅指定的事件
        /// </summary>
        /// <typeparam name="TEvent">事件的类型</typeparam>
        /// <typeparam name="TEventHandler">事件的处理程序类型或消费者的类型</typeparam>
        void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : IntegrationEvent
            where TEventHandler : IEventHandler<TEvent>;
    }
}
