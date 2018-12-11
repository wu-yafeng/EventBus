using System;
using System.Collections.Generic;

namespace Rabble.EventBus.Abstractions
{
    /// <summary>
    /// 表示一个可订阅的事件数据对象
    /// </summary>
    /// <remarks>
    /// 当你需要为任一一个事件增加字段，修改字段的时候，你应该重新定义一个新的事件，而不是修改原有的事件结构
    /// </remarks>
    public abstract class IntegrationEvent : IEquatable<IntegrationEvent>
    {
        /// <summary>
        /// 初始化 <see cref="IntegrationEvent"/> 类型的新实例。
        /// </summary>
        /// <param name="id"></param>
        protected IntegrationEvent(Guid id)
        {
            Id = id;
        }
        /// <summary>
        /// 该事件的唯一标识
        /// </summary>
        public Guid Id { get; }
        /// <summary>
        /// 比较指定的对象和当前对象是否相等
        /// </summary>
        /// <param name="obj">要比较的对象</param>
        /// <returns>如果相等，返回true</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as IntegrationEvent);
        }
        /// <summary>
        /// 比较指定的事件和当前事件是否是同一个事件
        /// </summary>
        /// <param name="other">要比较的对象</param>
        /// <returns>如果相等，返回true</returns>
        public bool Equals(IntegrationEvent other)
        {
            return other != null &&
                   Id.Equals(other.Id);
        }
        /// <summary>
        /// 计算当前事件的hashcode
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<Guid>.Default.GetHashCode(Id);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event1"></param>
        /// <param name="event2"></param>
        /// <returns></returns>
        public static bool operator ==(IntegrationEvent event1, IntegrationEvent event2)
        {
            return EqualityComparer<IntegrationEvent>.Default.Equals(event1, event2);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event1"></param>
        /// <param name="event2"></param>
        /// <returns></returns>
        public static bool operator !=(IntegrationEvent event1, IntegrationEvent event2)
        {
            return !(event1 == event2);
        }
        /// <summary>
        /// 获取当前对象的数据的 <see cref="string"/> 表示形式
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Id.ToString();
        }
    }
}