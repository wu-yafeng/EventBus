using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using Rabble.EventBus.RabbitMQ;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xunit;

namespace UnitTests.RabbitMQ
{
    public class RabbitMQEventBusTest : IDisposable
    {
        public RabbitMQEventBusTest()
        {
            _repository = new MockRepository(MockBehavior.Strict);

            _connectionMock = _repository.Create<IRabbitMQPersistentConnection>();
            _loggerMock = _repository.Create<ILogger<RabbitMQEventBus>>();
            _optionMock = _repository.Create<IOptions<RabbitMQEventBusOptions>>();
            _serviceProviderMock = _repository.Create<IServiceProvider>();
            _diagnositcSource = _repository.Create<DiagnosticSource>();
            _channelMock = _repository.Create<IModel>();
        }
        private readonly Mock<IRabbitMQPersistentConnection> _connectionMock;
        private readonly Mock<ILogger<RabbitMQEventBus>> _loggerMock;
        private readonly Mock<IOptions<RabbitMQEventBusOptions>> _optionMock;
        private readonly Mock<IServiceProvider> _serviceProviderMock;
        private readonly Mock<DiagnosticSource> _diagnositcSource;
        private readonly Mock<IModel> _channelMock;
        private readonly MockRepository _repository;
        private RabbitMQEventBus CreateEventbus()
        {
            _channelMock.Setup(x => x.ExchangeDeclare(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))
                .Verifiable();
            _channelMock.Setup(x => x.QueueDeclare(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>()))
                .Returns((string queue, bool durable, bool exclusive, bool autoDelete, IDictionary<string, object> arguments) => new QueueDeclareOk(queue, 0, 1))
                .Verifiable();

            _channelMock.Setup(x => x.BasicConsume(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<IDictionary<string, object>>(), It.IsAny<IBasicConsumer>()))
                .Returns("")
                .Verifiable();

            _optionMock.SetupGet(x => x.Value)
                .Returns(CreateOptions())
                .Verifiable();
            _connectionMock.Setup(x => x.TryConnect())
                .Returns(true)
                .Verifiable();
            _connectionMock.SetupGet(x => x.IsConnected)
                .Returns(false)
                .Verifiable();
            _connectionMock.Setup(x => x.CreateModel())
                .Returns(_channelMock.Object)
                .Verifiable();

            return new RabbitMQEventBus(_connectionMock.Object,
                _loggerMock.Object,
                _optionMock.Object,
                _serviceProviderMock.Object,
                _diagnositcSource.Object);
        }

        [Fact]
        public void CreateEventBus_Should_ConnectRabbitMQ()
        {


            var eventBus = CreateEventbus();

            Assert.NotNull(eventBus);
        }



        private RabbitMQEventBusOptions CreateOptions() => new RabbitMQEventBusOptions()
        {
            BrokerName = "XUNIT_BROKER",
            QueueName = "XUNIT_APP_QUEUE"
        };

        public void Dispose() => _repository.VerifyAll();
    }
}
