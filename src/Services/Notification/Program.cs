using OpenTelemetry.Resources;
using OpenTelemetry;
using System.Diagnostics;
using System.Reflection;
using OpenTelemetry.Trace;

namespace Notification;

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
                    .AddSource(ServiceName)
                    .SetResourceBuilder(GetResourceBuilder())
                    .AddOtlpExporter(options =>
                        options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
                    .Build();

        var app = builder.Build();

        using (var activity = ActivitySource.StartActivity("Notify"))
        {
            await Task.Delay(500);
        }

        app.Run();
    }

    private static ResourceBuilder GetResourceBuilder()
        => ResourceBuilder.CreateDefault().AddService(ServiceName, ServiceVersion);
}
