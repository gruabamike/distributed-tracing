using Apache.NMS;
using OpenTelemetry.Trace;
using System.Diagnostics;

namespace MessageBroker.ActiveMQ.ManualInstrumentation;

internal static class ActiveMQActivityTagHelper
{
    public static void SetMessagingActivityDetails(
        Activity activity,
        IConnectionFactory connectionFactory,
        IMessage message)
    {
        var (destinationName, destinationKind) = GetMessageDestinationValues(message.NMSDestination);
        activity.DisplayName = destinationName;

        activity.SetTag(TraceSemanticConventions.AttributeMessagingDestination, destinationName);
        activity.SetTag(TraceSemanticConventions.AttributeMessagingDestinationKind, destinationKind);
        activity.SetTag(TraceSemanticConventions.AttributeMessagingTempDestination, message.NMSDestination.IsTemporary);
        activity.SetTag(TraceSemanticConventions.AttributeMessagingUrl, connectionFactory?.BrokerUri?.ToString() ?? "unkown");
        activity.SetTag(TraceSemanticConventions.AttributeMessagingMessageId, message.NMSMessageId);
        activity.SetTag(TraceSemanticConventions.AttributeMessagingConversationId, message.NMSCorrelationID);
    }

    private static (string DestinationName, string DestinationKindValue) GetMessageDestinationValues(IDestination destination)
    {
        switch (destination.DestinationType)
        {
            case DestinationType.Topic:
            case DestinationType.TemporaryTopic:
                return (((ITopic)destination).TopicName, TraceSemanticConventions.MessagingDestinationKindValues.Topic);
            case DestinationType.Queue:
            case DestinationType.TemporaryQueue:
            default:
                return (((IQueue)destination).QueueName, TraceSemanticConventions.MessagingDestinationKindValues.Queue);
        }
    }
}
