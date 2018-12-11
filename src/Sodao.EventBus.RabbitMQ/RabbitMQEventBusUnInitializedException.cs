using System;
using System.Collections.Generic;
using System.Text;

namespace Sodao.EventBus.RabbitMQ
{
    /// <summary>
    /// 消息队列未初始化异常
    /// </summary>
    public class RabbitMQEventBusUnInitializedException : Exception
    {
        /// <summary>
        /// Initiazlied <see cref="RabbitMQEventBusUnInitializedException"/>
        /// </summary>
        public RabbitMQEventBusUnInitializedException(string eventName) : base(string.Format("The event '{0}' has been unsubscribed implicitly or eventbus is uninitialized,because there are no event handlers was found,if application unsubscribe event in newly version but subscribe in old version,plase call method 'IEventBus.Unsubscribe<TEvent,TEventHandler>()' to unsubscribe event explicitly in your newly application code", eventName))
        {
            
        }
    }
}
