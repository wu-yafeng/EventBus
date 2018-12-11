using Sodao.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sodao.EventBus.InMemory
{
    public class InMemoryEventBus : IEventBus
    {
        private readonly List<object> _observables;
        private readonly IServiceProvider _serviceContainer;
        private readonly IDictionary<Type, IDisposable> _disposer;
        public InMemoryEventBus(IServiceProvider serviceContainer)
        {
            _serviceContainer = serviceContainer ?? throw new ArgumentNullException(nameof(serviceContainer));
            _observables = new List<object>();
            _disposer = new Dictionary<Type, IDisposable>();
        }

        public void Publish<TEvent>(TEvent @event) where TEvent : IntegrationEvent
        {
            foreach (var o in _observables.OfType<EventBusObservable<TEvent>>())
            {
                o.Publish(@event);
            }
        }

        public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default(CancellationToken)) where TEvent : IntegrationEvent
        {
            return Task.Run(() => Publish(@event), cancellationToken);
        }

        public void Subscribe<TEvent, TEventHandler>()
            where TEvent : IntegrationEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            var observable = _observables.OfType<EventBusObservable<TEvent>>().ToList();
            if (observable.Any())
            {
                observable.First().Subscribe(new EventBusObserver<TEvent>(_serviceContainer, typeof(TEventHandler)));
            }
            else
            {
                var o = new EventBusObservable<TEvent>();
                _disposer.Add(typeof(TEventHandler), o.Subscribe(new EventBusObserver<TEvent>(_serviceContainer, typeof(TEventHandler))));
                _observables.Add(o);
            }
        }

        public void Unsubscribe<TEvent, TEventHandler>()
            where TEvent : IntegrationEvent
            where TEventHandler : IEventHandler<TEvent>
        {
            var observable = _observables.OfType<EventBusObservable<TEvent>>();
            if (observable.Any())
            {
                var disposer = _disposer.FirstOrDefault(x => x.Key == typeof(TEventHandler));

                if (disposer.Key != null)
                {
                    _disposer.Remove(disposer);
                    disposer.Value?.Dispose();
                }
            }
        }
    }
}
