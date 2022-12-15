using OpenTelemetry.Instrumentation.ActiveMQ.Implementation;
using OpenTelemetry.Trace;

namespace OpenTelemetry.Instrumentation.ActiveMQ;

public static class TracerProviderBuilderExtensions
{
    public static TracerProviderBuilder AddActiveMQInstrumentation(
        this TracerProviderBuilder builder,
        Action<ActiveMQInstrumentationOptions>? configureActiveMQInstrumentationOptions = null)
    {
        ArgumentNullException.ThrowIfNull(builder, nameof(builder));

        var activeMQOptions = new ActiveMQInstrumentationOptions();
        configureActiveMQInstrumentationOptions?.Invoke(activeMQOptions);

        builder.AddInstrumentation(() => new ActiveMQInstrumentation(activeMQOptions));
        builder.AddSource(ActiveMQSourceInfoProvider.ActivitySourceName);

        return builder;
    }
}
