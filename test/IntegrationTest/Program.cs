using Microsoft.Extensions.DependencyInjection;
using Sodao.EventBus.Abstractions;
using Sodao.EventBus.RabbitMQ;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IntegrationTest
{
    internal class Program
    {
        private static int PublishCount = 0;
        private static int HandleCount = 0;

        private static void Main(string[] args)
        {
            var ipaddress = Environment.GetEnvironmentVariable("EBS_HOST");

            if (string.IsNullOrWhiteSpace(ipaddress))
            {
                Console.WriteLine("Invalid HOST");
                return;
            }

            var port = 5672;

            if (!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("EBS_PORT")) && !int.TryParse(Environment.GetEnvironmentVariable("EBS_PORT"), out port))
            {
                Console.WriteLine("Invalid port");
                return;
            }

            var user = Environment.GetEnvironmentVariable("EBS_USER") ?? "guest";

            var password = Environment.GetEnvironmentVariable("EBS_PSW") ?? "guest";


            var container = new ServiceCollection();

            container
                .AddLogging()
                .AddRabbitMQEventBus(opt =>
            {
                opt.BrokerName = "integration_test_ebs";
                opt.QueueName = "INTEGRATION_TEST";
                opt.Host = ipaddress;
                opt.Password = password;
                opt.Port = port;
            });

            var ebs = container.BuildServiceProvider(false).GetRequiredService<IEventBus>();

            ebs.Subscribe(typeof(TestEventHandler).Assembly);

            ebs.Subscribe<TestEvent, TestEventHandler>();

            PublishCount++;

            ebs.Publish(new TestEvent(Guid.NewGuid()));

            if (SpinWait.SpinUntil(() => PublishCount == HandleCount, TimeSpan.FromSeconds(5)))
            {
                Console.WriteLine("Test pass successfully");
            }
            Console.WriteLine("Test runs timeout");
        }

        private class TestEvent : IntegrationEvent
        {
            public TestEvent(Guid id) : base(id)
            {
                IVal = int.MaxValue;
                SVal = string.Empty;
                IVals = new[]
                {
                    int.MaxValue,
                    int.MinValue,
                };

                DVal = DateTime.Now;

            }

            public int IVal { get; set; }

            public string SVal { get; set; }

            public int[] IVals { get; set; }

            public string[] SVals { get; set; }

            public DateTime DVal { get; set; }
        }

        private class TestEventHandler : IEventHandler<TestEvent>
        {
            public async Task HandleAsync(TestEvent @event, CancellationToken cancellationToken = default(CancellationToken))
            {
                HandleCount++;

                await Task.CompletedTask;
            }
        }
    }
}
