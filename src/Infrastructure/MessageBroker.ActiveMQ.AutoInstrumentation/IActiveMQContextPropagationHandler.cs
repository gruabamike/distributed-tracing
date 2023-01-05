using Apache.NMS;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

public interface IActiveMQContextPropagationHandler
{
    void Inject(ActivityContext contextToInject, IMessage message);

    PropagationContext Extract(IMessage message);
}
