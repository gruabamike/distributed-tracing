﻿using MessageBroker.ActiveMQ.ManualInstrumentation;
using MessageBroker.Contract;
using OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Fulfillment;

internal class Program
{
    private const string BROKER_URI_ENVIRONMENT_VARIABLE_NAME = "BrokerUri";
    private const string ORDER_QUEUE_NAME_ENVIRONMENT_VARIABLE_NAME = "OrderProcessingQueueName";
    private const string NOTIFICATION_QUEUE_NAME_ENVIRONMENT_VARIABLE_NAME = "NotificationProcessingQueueName";

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
        await messageReceiver.StartReceiveAsync();

        app.Run();
    }

    private static ResourceBuilder GetResourceBuilder()
        => ResourceBuilder.CreateDefault().AddService(ServiceName, ServiceVersion);
}
