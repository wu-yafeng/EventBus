using RabbitMQ.Client.Events;
using Rabble.EventBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Rabble.EventBus.RabbitMQ
{
    /// <summary>
    /// 遥测扩展
    /// </summary>
    internal static class DiagnosticSourceExtensions
    {
        public static void BeforePublushEvent(this DiagnosticSource diagnosticSource, IntegrationEvent @event)
        {
            if (diagnosticSource == null)
            {
                throw new ArgumentNullException(nameof(diagnosticSource));
            }

            if (diagnosticSource.IsEnabled(RabbitMQEventBusDiagnosticEventConstant.BEFORE_PUBLISH))
            {
                diagnosticSource.Write(RabbitMQEventBusDiagnosticEventConstant.BEFORE_PUBLISH, new { @event });
            }
        }
        public static void AfterPublushEvent(this DiagnosticSource diagnosticSource, IntegrationEvent @event)
        {
            if (diagnosticSource == null)
            {
                throw new ArgumentNullException(nameof(diagnosticSource));
            }

            if (diagnosticSource.IsEnabled(RabbitMQEventBusDiagnosticEventConstant.AFTER_PUBLISH))
            {
                diagnosticSource.Write(RabbitMQEventBusDiagnosticEventConstant.AFTER_PUBLISH, new { @event });
            }
        }

        public static void PublishRetry(this DiagnosticSource diagnosticSource, IntegrationEvent @event, Exception exception, int time)
        {
            if (diagnosticSource == null)
            {
                throw new ArgumentNullException(nameof(diagnosticSource));
            }

            if (diagnosticSource.IsEnabled(RabbitMQEventBusDiagnosticEventConstant.PUBLISH_RETRY))
            {
                diagnosticSource.Write(RabbitMQEventBusDiagnosticEventConstant.PUBLISH_RETRY, new { @event, exception, time });
            }
        }

        public static void PublishFail(this DiagnosticSource diagnosticSource, IntegrationEvent @event, Exception exception)
        {
            if (diagnosticSource == null)
            {
                throw new ArgumentNullException(nameof(diagnosticSource));
            }

            if (diagnosticSource.IsEnabled(RabbitMQEventBusDiagnosticEventConstant.PUBLISH_FAIL))
            {
                diagnosticSource.Write(RabbitMQEventBusDiagnosticEventConstant.PUBLISH_FAIL, new { @event, exception });
            }
        }
        public static void PublishRetryFailed(this DiagnosticSource diagnosticSource, IntegrationEvent @event)
        {
            if (diagnosticSource == null)
            {
                throw new ArgumentNullException(nameof(diagnosticSource));
            }

            if (diagnosticSource.IsEnabled(RabbitMQEventBusDiagnosticEventConstant.PUBLISH_RETRY_FAIL))
            {
                diagnosticSource.Write(RabbitMQEventBusDiagnosticEventConstant.PUBLISH_RETRY_FAIL, new { @event });
            }
        }

        public static void BeforeReceived(this DiagnosticSource diagnosticSource, object sender, BasicDeliverEventArgs args)
        {
            if (diagnosticSource == null)
            {
                throw new ArgumentNullException(nameof(diagnosticSource));
            }

            if (diagnosticSource.IsEnabled(RabbitMQEventBusDiagnosticEventConstant.BEFORE_HANDLE_MESSAGE))
            {
                diagnosticSource.Write(RabbitMQEventBusDiagnosticEventConstant.BEFORE_HANDLE_MESSAGE, new { sender, args });
            }
        }
        public static void AfterReceived(this DiagnosticSource diagnosticSource, BasicDeliverEventArgs args)
        {
            if (diagnosticSource == null)
            {
                throw new ArgumentNullException(nameof(diagnosticSource));
            }

            if (diagnosticSource.IsEnabled(RabbitMQEventBusDiagnosticEventConstant.AFTER_HANDLE_MESSAGE))
            {
                diagnosticSource.Write(RabbitMQEventBusDiagnosticEventConstant.AFTER_HANDLE_MESSAGE, new { args });
            }
        }
    }
}
