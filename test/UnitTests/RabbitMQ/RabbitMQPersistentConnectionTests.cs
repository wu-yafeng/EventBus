using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using RabbitMQ.Client;
using Rabble.EventBus.RabbitMQ;
using System;
using Xunit;

namespace UnitTests.RabbitMQ
{
    public class RabbitMQPersistentConnectionTests : IDisposable
    {
        private MockRepository _mockRepository;

        private Mock<ILogger<RabbitMQPersistentConnection>> _loggerMock;
        private Mock<IOptions<RabbitMQEventBusOptions>> _optionsMock;
        private Mock<IConnectionFactory> _connectionFactoryMock;

        public RabbitMQPersistentConnectionTests()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);

            _loggerMock = _mockRepository.Create<ILogger<RabbitMQPersistentConnection>>();
            _optionsMock = _mockRepository.Create<IOptions<RabbitMQEventBusOptions>>();
            _connectionFactoryMock = _mockRepository.Create<IConnectionFactory>();
        }

        public void Dispose() => _mockRepository.VerifyAll();

        private RabbitMQPersistentConnection CreateRabbitMQPersistentConnection() => new RabbitMQPersistentConnection(
                _loggerMock.Object,
                _optionsMock.Object,
                _connectionFactoryMock.Object);

        [Fact]
        public void TryConnect_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            _optionsMock.Setup(x => x.Value)
                .Returns(RabbitMQOptionsProvider.CreateOptions())
                .Verifiable();
            _loggerMock.Setup(x => x.Log(It.Is<LogLevel>(l => l == LogLevel.Information), It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<object, Exception, string>>()))
                .Verifiable();

            var connectionMock = _mockRepository.Create<IConnection>();

            connectionMock.Setup(x => x.IsOpen)
                .Returns(true)
                .Verifiable();

            connectionMock.SetupGet(x => x.Endpoint)
                .Returns(new AmqpTcpEndpoint() { HostName = "host" })
                .Verifiable();

            _connectionFactoryMock.Setup(x => x.CreateConnection())
                .Returns(connectionMock.Object)
                .Verifiable();
            var tester = CreateRabbitMQPersistentConnection();

            // Act
            var result = tester.TryConnect();

            // Assert
            Assert.True(result);
        }
    }
}
