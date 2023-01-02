using Apache.NMS;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

namespace MessageBroker.ActiveMQ.ManualInstrumentation;

public sealed class ActiveMQContextPropagationHandler : IActiveMQContextPropagationHandler
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public void Inject(ActivityContext contextToInject, IMessage message)
        => Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), message.Properties, InjectTraceContext);

    public PropagationContext Extract(IMessage message)
    {
        var parentContext = Propagator.Extract(default, message.Properties, ExtractTraceContext);
        Baggage.Current = parentContext.Baggage;

        return parentContext;
    }

    private void InjectTraceContext(IPrimitiveMap messageProperties, string key, string value)
        => messageProperties.SetString(key, value);

    private IEnumerable<string> ExtractTraceContext(IPrimitiveMap messageProperties, string key)
    {
        if (messageProperties.Contains(key))
        {
            return new[] { messageProperties.GetString(key) };
        }

        return Enumerable.Empty<string>();
    }
}
