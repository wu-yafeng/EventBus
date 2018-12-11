using Sodao.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sodao.EventBus.InMemory
{
    internal class EventBusObservable<T> : IObservable<T>
        where T : IntegrationEvent
    {
        private readonly List<IObserver<T>> _subscriptions;

        public EventBusObservable()
        {
            _subscriptions = new List<IObserver<T>>();
        }

        public void Publish(T @event)
        {
            try
            {
                foreach (var subscription in _subscriptions)
                {
                    subscription.OnNext(@event);
                }
                foreach (var subscription in _subscriptions)
                {
                    subscription.OnCompleted();
                }
            }
            catch (Exception ex)
            {
                foreach (var subscription in _subscriptions)
                {
                    subscription.OnError(ex);
                }
            }
           
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            _subscriptions.Add(observer);

            return new EventBusObservableDisposer(_subscriptions, observer);
        }

        private class EventBusObservableDisposer : IDisposable
        {
            private readonly List<IObserver<T>> _source;
            private readonly IObserver<T> _target;

            public EventBusObservableDisposer(List<IObserver<T>> source, IObserver<T> target)
            {
                _source = source ?? throw new ArgumentNullException(nameof(source));
                _target = target ?? throw new ArgumentNullException(nameof(target));
            }

            public void Dispose()
            {
                _source.Remove(_target);
            }
        }
    }
}
