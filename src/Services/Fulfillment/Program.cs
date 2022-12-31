using MessageBroker.ActiveMQ.ManualInstrumentation;
using MessageBroker.Contract;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;

namespace Fulfillment;

internal class Program
{
    private static readonly AssemblyName AssemblyName = typeof(Program).Assembly.GetName();
    private static readonly string ServiceName = AssemblyName.Name!;
    private static readonly string ServiceVersion = AssemblyName?.Version?.ToString() ?? "1.0.0";

    private static readonly ActivitySource ActivitySource = new ActivitySource(ServiceName, ServiceVersion);

    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .AddSource(ServiceName, ActiveMQSourceInfoProvider.ActivitySourceName)
                    .SetResourceBuilder(GetResourceBuilder())
                    .AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
                    .Build();
        /*
        builder.Services.AddOpenTelemetry()
            .WithTracing(builder => builder
                .AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
                .AddSource(serviceName, nameof(ActiveMQInstrumentationBroker))
                .SetResourceBuilder(
                    ResourceBuilder.CreateDefault()
                        .AddService(serviceName, serviceVersion: serviceVersion))
            )
            .StartWithHost();
        */

        IMessageReceiver messageReceiver = new MessageReceiver(
            new Uri("activemq:tcp://localhost:61616"),
            "OrderProcessing",
            async (IMessage message) =>
            {
                using (var activity = ActivitySource.StartActivity("Fulfillment"))
                {
                    IFulfillmentManager fulfillmentManager = new FulfillmentManager();
                    await fulfillmentManager.Fulfill();
                }

                // TODO: invoke notify
            });

        var app = builder.Build();

        app.Run();
    }

    private static ResourceBuilder GetResourceBuilder()
        => ResourceBuilder.CreateDefault().AddService(ServiceName, ServiceVersion);
}
