﻿using MessageBroker.ActiveMQ.AutoInstrumentation;
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
    private const string NOTIFICATION_TOPIC_NAME_ENVIRONMENT_VARIABLE_NAME = "NOTIFICATION_PROCESSING_TOPIC_NAME";

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
        }).CreateLogger(ServiceName);

        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
                    .SetResourceBuilder(GetResourceBuilder())
                    .AddSource(ServiceName)
                    .AddOtlpExporter(options => options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf)
                    .AddActiveMQInstrumentation()
                    .Build();

        var (brokerUri, orderProcessingQueueName, notificationProcessingTopicName) = (
            new Uri(Environment.GetEnvironmentVariable(BROKER_URI_ENVIRONMENT_VARIABLE_NAME)!),
            Environment.GetEnvironmentVariable(ORDER_QUEUE_NAME_ENVIRONMENT_VARIABLE_NAME)!,
            Environment.GetEnvironmentVariable(NOTIFICATION_TOPIC_NAME_ENVIRONMENT_VARIABLE_NAME)!);

        var app = builder.Build();

        using ActiveMQConnection activeMQConnection = new ActiveMQConnection(brokerUri, ServiceName);
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

        await retryPolicy.ExecuteAsync(() => activeMQConnection.OpenAsync());
        logger.LogInformation("successfully connected to message broker");

        using IMessageReceiver messageReceiver = new MessageReceiver(activeMQConnection);
        await messageReceiver.StartReceiveQueueAsync(
            orderProcessingQueueName,
            async (IMessage message) =>
            {
                using (var activity = ActivitySource.StartActivity("Do fulfillment"))
                {
                    IFulfillmentManager fulfillmentManager = new FulfillmentManager();
                    await fulfillmentManager.Fulfill();
                }

                using (var activity = ActivitySource.StartActivity("Send to notification topic"))
                {
                    IMessageSender messageSender = new MessageSender(activeMQConnection);
                    await messageSender.SendTopicAsync(notificationProcessingTopicName, new TextMessage(message.Content));
                }
            });

        app.Run();
    }

    private static ResourceBuilder GetResourceBuilder()
        => ResourceBuilder.CreateDefault().AddService(
            serviceName: ServiceName,
            serviceNamespace: ServiceName,
            serviceVersion: ServiceVersion);
}
