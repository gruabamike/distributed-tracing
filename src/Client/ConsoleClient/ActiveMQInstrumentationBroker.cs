using Apache.NMS;
using Apache.NMS.ActiveMQ;
using MySql.Data.MySqlClient;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace ConsoleClient;

public class ActiveMQInstrumentationBroker
{
    private const string ServiceName = $"{nameof(ActiveMQInstrumentationBroker)}";
    private const string ServiceVersion = "1.0.0";

    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
    private static readonly ActivitySource ActivitySource = new ActivitySource(ServiceName, ServiceVersion);
    private static readonly Uri MessageBrokerConnectionUri = new Uri("activemq:tcp://localhost:61616");

    private const string QueueName = "OrderProcessor";

    private static async Task Main(string[] args)
    {
        using var tracerProvider = Sdk.CreateTracerProviderBuilder()
            .AddSource(ServiceName)
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService(serviceName: ServiceName, serviceVersion: ServiceVersion))
            .AddConsoleExporter()
            .Build();

        await ProduceAsync();
        await ConsumeAsync();
    }

    public async static Task ProduceAsync()
    {
        var activityName = "Publish";
        using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Producer);

        ActivityContext contextToInject = default;
        if (activity is not null)
        {
            contextToInject = activity.Context;
        }
        else if (Activity.Current is not null)
        {
            contextToInject = Activity.Current.Context;
        }
        //ActivityContext contextToInject = activity?.Context ?? Activity.Current?.Context ?? default;

        IConnectionFactory connectionFactory = new ConnectionFactory(MessageBrokerConnectionUri);
        using IConnection connection = await connectionFactory.CreateConnectionAsync();
        await connection.StartAsync();
        using ISession session = await connection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        using IDestination destination = await session.GetQueueAsync(QueueName);

        using IMessageProducer producer = await session.CreateProducerAsync(destination);
        producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
        ITextMessage message = await session.CreateTextMessageAsync("Test message!");
        message.Properties["hi"] = "ok";

        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), message.Properties, InjectTraceContext);
        AddMessageBrokerTags(activity);

        await producer.SendAsync(message);
        Console.WriteLine($">> msg produced!");
    }

    private static void InjectTraceContext(IPrimitiveMap messageProperties, string key, string value)
    {
        messageProperties.SetString(key, value);
    }

    public async static Task ConsumeAsync()
    {
        IConnectionFactory connectionFactory = new ConnectionFactory(MessageBrokerConnectionUri);
        using IConnection connection = await connectionFactory.CreateConnectionAsync();
        await connection.StartAsync();
        using ISession session = await connection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        using IDestination destination = await session.GetQueueAsync(QueueName);

        using IMessageConsumer consumer = await session.CreateConsumerAsync(destination);
        IMessage message = await consumer.ReceiveAsync(TimeSpan.FromMinutes(3));
        var textMessage = message as ITextMessage;

        if (textMessage is not null)
        {
            Console.WriteLine($">> msg consumed: {textMessage.Text}");

            var parentContext = Propagator.Extract(default, message.Properties, ExtractTraceContext);
            Baggage.Current = parentContext.Baggage;

            var activityName = "Consume";
            using var activity = ActivitySource.StartActivity(activityName, ActivityKind.Consumer, parentContext.ActivityContext);
            AddMessageBrokerTags(activity);
        }
    }

    private static IEnumerable<string> ExtractTraceContext(IPrimitiveMap messageProperties, string key)
    {
        try
        {
            if (messageProperties.Contains(key))
            {
                return new[] { messageProperties.GetString(key) };
            }
        }
        catch (Exception ex)
        {
            // TODO: Logging
        }

        return Enumerable.Empty<string>();
    }

    private static void AddMessageBrokerTags(Activity? activity)
    {
        activity?.SetTag("messaging.system", "activemq");
        activity?.SetTag("messaging.destination_kind", "queue");
        activity?.SetTag("messaging.destination", QueueName);
        activity?.SetTag("messaging.activemq_customTag", "test123");
    }
}