using Microsoft.Extensions.DependencyInjection;
using Sodao.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sodao.EventBus.InMemory
{
    internal class EventBusObserver<T> : IObserver<T>
        where T : IntegrationEvent
    {
        private readonly IServiceProvider _container;
        private readonly Type _handlerType;

        public EventBusObserver(IServiceProvider container, Type handlerType)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _handlerType = handlerType ?? throw new ArgumentNullException(nameof(handlerType));
        }

        public void OnCompleted()
        {

        }

        public void OnError(Exception error)
        {

        }

        public void OnNext(T value)
        {
            using (var scope = _container.CreateScope())
            {
                var handler = (IEventHandler<T>)ActivatorUtilities.GetServiceOrCreateInstance(scope.ServiceProvider, _handlerType);

                handler.HandleAsync(value).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }
}
