using MessageBroker.ActiveMQ.ManualInstrumentation;
using MessageBroker.Contract;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using System.Diagnostics;
using System.Reflection;

namespace Fulfillment;

internal class Program
{
    private const int MAX_BROKER_CONNETION_RETRIES = 5;

    private const string BROKER_URI_ENVIRONMENT_VARIABLE_NAME = "BROKER_URI";
    private const string ORDER_QUEUE_NAME_ENVIRONMENT_VARIABLE_NAME = "ORDER_PROCESSING_QUEUE_NAME";
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

        var logger = LoggerFactory.Create(config =>
        {
            config.AddConsole();
            config.AddConfiguration(builder.Configuration.GetSection("Logging"));
        }).CreateLogger(nameof(Fulfillment));

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .AddSource(ServiceName, ActiveMQSourceInfoProvider.ActivitySourceName)
                    .SetResourceBuilder(GetResourceBuilder())
                    .AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
                    .Build();

        var (brokerUri, orderProcessingQueueName, notificationProcessingQueueName) = (
            new Uri(Environment.GetEnvironmentVariable(BROKER_URI_ENVIRONMENT_VARIABLE_NAME)!),
            Environment.GetEnvironmentVariable(ORDER_QUEUE_NAME_ENVIRONMENT_VARIABLE_NAME)!,
            Environment.GetEnvironmentVariable(NOTIFICATION_QUEUE_NAME_ENVIRONMENT_VARIABLE_NAME)!);

        using IMessageReceiver messageReceiver = new MessageReceiver(
            new ActiveMQContextPropagationHandler(),
            brokerUri,
            orderProcessingQueueName,
            async (IMessage message) =>
            {
                using (var activity = ActivitySource.StartActivity("Do fulfillment"))
                {
                    IFulfillmentManager fulfillmentManager = new FulfillmentManager();
                    await fulfillmentManager.Fulfill();
                }

                using (var activity = ActivitySource.StartActivity("Send notification"))
                {
                    IMessageSender messageSender = new MessageSender(
                        new ActiveMQContextPropagationHandler(),
                        brokerUri,
                        notificationProcessingQueueName);
                    await messageSender.SendAsync(new TextMessage(message.Content));
                }
            });

        var app = builder.Build();

        var retryPolicy = Policy
            .Handle<Apache.NMS.NMSConnectionException>()
            .WaitAndRetryAsync(
                MAX_BROKER_CONNETION_RETRIES,
                (retryAttempt) =>
                {
                    var sleepDuration = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) / 2);
                    logger.LogInformation($"retry connecting to message broker (#{retryAttempt}; sleepDuration: {sleepDuration.Seconds}s)");
                    return sleepDuration;
                });

        await retryPolicy.ExecuteAsync(() => messageReceiver.StartReceiveAsync());
        logger.LogInformation("successfully connected to message broker");

        app.Run();
    }

    private static ResourceBuilder GetResourceBuilder()
        => ResourceBuilder.CreateDefault().AddService(ServiceName, ServiceVersion);
}
