using System.Threading;
using System.Threading.Tasks;

namespace Sodao.EventBus.Abstractions
{
    /// <summary>
    /// 表示处理指定 <see cref="IEventBus"/> 所分发的特定事件的处理程序和消费者
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<TEvent>
        where TEvent : IntegrationEvent
    {
        /// <summary>
        /// 以异步的方式处理事件
        /// </summary>
        /// <param name="event">事件</param>
        /// <param name="cancellationToken">用于取消事件的token</param>
        /// <returns></returns>
        Task HandleAsync(TEvent @event, CancellationToken cancellationToken = default(CancellationToken));
    }
}