using OpenTelemetry.Resources;
using OpenTelemetry;
using System.Diagnostics;
using System.Reflection;
using OpenTelemetry.Trace;
using MessageBroker.Contract;
using MessageBroker.ActiveMQ.ManualInstrumentation;
using Notification.Messages;

namespace Notification;

internal class Program
{
    private const string BROKER_URI_ENVIRONMENT_VARIABLE_NAME = "BROKER_URI";
    private const string NOTIFICATION_QUEUE_NAME_ENVIRONMENT_VARIABLE_NAME = "NOTIFICATION_PROCESSING_QUEUE_NAME";

    private static readonly AssemblyName AssemblyName = typeof(Program).Assembly.GetName();
    private static readonly string ServiceName = AssemblyName.Name!;
    private static readonly string ServiceVersion = AssemblyName?.Version?.ToString() ?? "1.0.0";

    private static readonly ActivitySource ActivitySource = new ActivitySource(ServiceName, ServiceVersion);

    private static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true)
            .AddJsonFile($"appsettings.{environment}.json", true)
            .AddEnvironmentVariables()
            .Build();

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .AddSource(ServiceName, ActiveMQSourceInfoProvider.ActivitySourceName)
                    .SetResourceBuilder(GetResourceBuilder())
                    .AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
                    .Build();

        var (brokerUri, notificationProcessingQueueName) = (
            new Uri(Environment.GetEnvironmentVariable(BROKER_URI_ENVIRONMENT_VARIABLE_NAME)!),
            Environment.GetEnvironmentVariable(NOTIFICATION_QUEUE_NAME_ENVIRONMENT_VARIABLE_NAME)!);

        using IMessageReceiver messageReceiver = new MessageReceiver(
            new ActiveMQContextPropagationHandler(),
            brokerUri,
            notificationProcessingQueueName,
            async (IMessage message) =>
            {
                using (var activity = ActivitySource.StartActivity("Sending mail notification"))
                {
                    INotificationManager notificationManager = new NotificationManager();
                    await notificationManager.Notify(new NotificationMessage(message.Content, message.Content));
                }
            });

        var app = builder.Build();

        await messageReceiver.StartReceiveAsync();
        app.Run();
    }

    private static ResourceBuilder GetResourceBuilder()
        => ResourceBuilder.CreateDefault().AddService(ServiceName, ServiceVersion);
}
