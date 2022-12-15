using OpenTelemetry.Instrumentation.ActiveMQ;

namespace OpenTelemetry.Instrumentation;

internal sealed class ActiveMQInstrumentation : IDisposable
{
    internal const string ActiveMQDiagnosticListenerName = "ActiveMQDiagnosticListener";

    //private readonly DiagnosticSourceSubscriber diagnosticSourceSubscriber;

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    private ActiveMQInstrumentationOptions activeMQOptions;

    public ActiveMQInstrumentation(ActiveMQInstrumentationOptions activeMQOptions)
    {
        this.activeMQOptions = activeMQOptions;
    }
}