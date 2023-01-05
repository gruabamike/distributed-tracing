using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

public sealed class ActiveMQSourceInfoProvider
{


    public static readonly AssemblyName AssemblyName = typeof(ActiveMQSourceInfoProvider).Assembly.GetName();
    public static readonly string ActivitySourceName = AssemblyName.Name!;
    public static readonly string Version = AssemblyName?.Version?.ToString() ?? "1.0.0";

    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName, Version);

    internal const string ApacheActiveMQSystemName = "activemq";
    internal static readonly string SendMessageActivityName = ActivitySourceName + ".Send";
    internal static readonly string ReceiveMessageActivityName = ActivitySourceName + ".Receive";

    internal static readonly IEnumerable<KeyValuePair<string, object>> ActivityCreationTags = new[]
    {
        new KeyValuePair<string, object>(TraceSemanticConventions.AttributeMessagingSystem, ApacheActiveMQSystemName),
    };
}
