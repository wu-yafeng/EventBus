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

        public void Dispose()
        {
            _mockRepository.VerifyAll();
        }

        private RabbitMQPersistentConnection CreateRabbitMQPersistentConnection()
        {
            return new RabbitMQPersistentConnection(
                _loggerMock.Object,
                _optionsMock.Object,
                _connectionFactoryMock.Object);
        }

        [Fact]
        public void TryConnect_StateUnderTest_ExpectedBehavior()
        {
            // Arrange
            var tester = this.CreateRabbitMQPersistentConnection();

            // Act
            var result = tester.TryConnect();

            // Assert
            Assert.True(false);
        }
    }
}
