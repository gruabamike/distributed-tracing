using OpenTelemetry.Trace;
using System.Diagnostics;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

internal static class ActiveMQActivityTagHelper
{
    public static void AddMessageBrokerTags(
        Activity? activity,
        string messageSystem,
        string destinationKind,
        string destination)
    {
        activity?.SetTag(TraceSemanticConventions.AttributeMessagingSystem, messageSystem);
        activity?.SetTag(TraceSemanticConventions.AttributeMessagingDestinationKind, destinationKind);
        activity?.SetTag(TraceSemanticConventions.AttributeMessagingDestination, destination);
    }
}
