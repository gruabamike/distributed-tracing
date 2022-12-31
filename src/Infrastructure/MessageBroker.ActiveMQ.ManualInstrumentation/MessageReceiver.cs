using Apache.NMS.ActiveMQ;
using Apache.NMS;
using System.Diagnostics;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;

namespace MessageBroker.ActiveMQ.ManualInstrumentation;

public class MessageReceiver : Contract.IMessageReceiver
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    private readonly Uri messageBrokerUri = new Uri("activemq:tcp://localhost:61616");
    private readonly string queueName;
    private readonly Func<Contract.IMessage, Task> messageHandler;

    public MessageReceiver(
        Uri messageBrokerUri,
        string queueName,
        Func<Contract.IMessage, Task> messageHandler)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException($"'{nameof(queueName)}' cannot be null or whitespace.", nameof(queueName));
        }

        this.messageBrokerUri = messageBrokerUri ?? throw new ArgumentNullException(nameof(messageBrokerUri));
        this.queueName = queueName;
        this.messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
    }

    public async Task ReceiveAsync()
    {
        IConnectionFactory connectionFactory = new ConnectionFactory(messageBrokerUri);
        using IConnection connection = await connectionFactory.CreateConnectionAsync();
        await connection.StartAsync();
        using ISession session = await connection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        using IDestination destination = await session.GetQueueAsync(queueName);

        using IMessageConsumer consumer = await session.CreateConsumerAsync(destination);
        consumer.Listener += OnMessageReceived;
    }

    private void OnMessageReceived(IMessage message)
    {
        var parentContext = Propagator.Extract(default, message.Properties, ExtractTraceContext);
        Baggage.Current = parentContext.Baggage;

        using var activity = ActiveMQSourceInfoProvider.ActivitySource.StartActivity(
            ActiveMQSourceInfoProvider.ReceiveMessageActivityName,
            ActivityKind.Consumer,
            parentContext.ActivityContext,
            ActiveMQSourceInfoProvider.ActivityCreationTags!);

        AddMessageBrokerTags(activity);
        messageHandler(new Contract.TextMessage(message?.ToString() ?? string.Empty));
    }

    private IEnumerable<string> ExtractTraceContext(IPrimitiveMap messageProperties, string key)
    {
        try
        {
            if (messageProperties.Contains(key))
            {
                return new[] { messageProperties.GetString(key) };
            }
        }
        catch (Exception)
        {
            // TODO: Logging
        }

        return Enumerable.Empty<string>();
    }

    private void AddMessageBrokerTags(Activity? activity)
    {
        activity?.SetTag("messaging.system", "activemq");
        activity?.SetTag("messaging.destination_kind", "queue");
        activity?.SetTag("messaging.destination", queueName);
        activity?.SetTag("messaging.activemq_customTag", "test123");
    }
}
