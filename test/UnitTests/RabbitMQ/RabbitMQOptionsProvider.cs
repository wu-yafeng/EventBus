using Rabble.EventBus.RabbitMQ;

namespace UnitTests.RabbitMQ
{
    public class RabbitMQOptionsProvider
    {
        public static RabbitMQEventBusOptions CreateOptions() => new RabbitMQEventBusOptions()
        {
            BrokerName = "XUNIT_BROKER",
            QueueName = "XUNIT_APP_QUEUE"
        };
    }
}
