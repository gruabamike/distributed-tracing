using Apache.NMS;
using Apache.NMS.ActiveMQ;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;
using System.Text;

namespace ConsoleClient;

internal class Program
{
    private static ActivitySource activitySource = new ActivitySource("ConsoleClient.Program", "1.0.0");
    private const string QueueName = "TestQueue";
    private static readonly Uri messageBrokerConnectionUri = new Uri("activemq:tcp://localhost:61616");

    private static async Task Main(string[] args)
    {
        await ProduceAsync();
        await ConsumeAsync();
    }

    private async static Task ProduceAsync()
    {
        using var activity = activitySource.StartActivity("Sending message", ActivityKind.Producer);
        IConnectionFactory connectionFactory = new ConnectionFactory(messageBrokerConnectionUri);
        using IConnection connection = await connectionFactory.CreateConnectionAsync();
        await connection.StartAsync();
        using ISession session = await connection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        using IDestination destination = await session.GetQueueAsync(QueueName);

        using IMessageProducer producer = await session.CreateProducerAsync(destination);
        producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

        ITextMessage message = await session.CreateTextMessageAsync("Test message!");
        ActivityContext contextToInject = activity?.Context ?? Activity.Current?.Context ?? default;
        TraceContextPropagator propagator = new TraceContextPropagator();
        propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), message.Properties, InjectTraceContext);

        await producer.SendAsync(message);
        Console.WriteLine($">> msg produced!");
    }

    private static void InjectTraceContext(IPrimitiveMap messageProperties, string key, string value)
    {
        messageProperties[key] = value;
    }

    private async static Task ConsumeAsync()
    {
        IConnectionFactory connectionFactory = new ConnectionFactory(messageBrokerConnectionUri);
        using IConnection connection = await connectionFactory.CreateConnectionAsync();
        await connection.StartAsync();
        using ISession session = await connection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        using IDestination destination = await session.GetQueueAsync(QueueName);

        using IMessageConsumer consumer = await session.CreateConsumerAsync(destination);
        IMessage message = await consumer.ReceiveAsync(TimeSpan.FromMilliseconds(1000));
        var textMessage = message as ITextMessage;

        if (textMessage is not null)
        {
            Console.WriteLine($">> msg consumed: {textMessage.Text}");
        }

        TraceContextPropagator propagator = new TraceContextPropagator();
        var parentContext = propagator.Extract(default, message.Properties, ExtractTraceContext);
        Baggage.Current = parentContext.Baggage;

        using var activity = activitySource.StartActivity("Consuming message", ActivityKind.Consumer, parentContext.ActivityContext);
    }

    private static IEnumerable<string> ExtractTraceContext(IPrimitiveMap messageProperties, string key)
    {
        try
        {
            if (messageProperties[key] is not null)
            {
                var value = messageProperties[key] as byte[];
                return new[] { Encoding.UTF8.GetString(value) };
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