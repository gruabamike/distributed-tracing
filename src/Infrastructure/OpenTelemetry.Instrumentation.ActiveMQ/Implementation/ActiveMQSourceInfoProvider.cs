using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;

namespace OpenTelemetry.Instrumentation.ActiveMQ.Implementation;

/// <summary>
/// Provider class that holds common properties used by ActiveMQDiagnosticListener on .NET Core
/// </summary>
internal sealed class ActiveMQSourceInfoProvider
{
    public const string ApacheActiveMQSystemName = "activemq";

    public static readonly AssemblyName AssemblyName = typeof(ActiveMQSourceInfoProvider).Assembly.GetName();
    public static readonly string ActivitySourceName = AssemblyName.Name!;
    public static readonly Version Version = AssemblyName.Version!;
    public static readonly ActivitySource ActivitySource = new(ActivitySourceName, Version.ToString());
    public static readonly string ActivityName = ActivitySourceName + ".Execute";

    public static readonly IEnumerable<KeyValuePair<string, object>> CreationTags = new[]
    {
        new KeyValuePair<string, object>(TraceSemanticConventions.AttributeMessagingSystem, ApacheActiveMQSystemName),
    };
}
