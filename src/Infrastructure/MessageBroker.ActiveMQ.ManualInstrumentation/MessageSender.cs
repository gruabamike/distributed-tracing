using System.Diagnostics;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using OpenTelemetry.Context.Propagation;
using OpenTelemetry;

namespace MessageBroker.ActiveMQ.ManualInstrumentation;

public class MessageSender : Contract.IMessageSender // TODO: , IAsyncDisposable
{
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;
    private readonly Uri messageBrokerUri = new Uri("activemq:tcp://localhost:61616");
    private readonly string queueName;

    public MessageSender(
        Uri messageBrokerUri,
        string queueName)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException($"'{nameof(queueName)}' cannot be null or whitespace.", nameof(queueName));
        }

        this.messageBrokerUri = messageBrokerUri ?? throw new ArgumentNullException(nameof(messageBrokerUri));
        this.queueName = queueName;
    }

    public async Task SendAsync(Contract.IMessage message)
    {
        using var activity = ActiveMQSourceInfoProvider.ActivitySource.StartActivity(
            name: ActiveMQSourceInfoProvider.SendMessageActivityName,
            kind: ActivityKind.Producer,
            tags: ActiveMQSourceInfoProvider.ActivityCreationTags!);

        ActivityContext contextToInject = default;
        if (activity is not null)
        {
            contextToInject = activity.Context;
        }
        else if (Activity.Current is not null)
        {
            contextToInject = Activity.Current.Context;
        }

        IConnectionFactory connectionFactory = new ConnectionFactory(messageBrokerUri);
        using IConnection connection = await connectionFactory.CreateConnectionAsync();
        await connection.StartAsync();
        using ISession session = await connection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        using IDestination destination = await session.GetQueueAsync(queueName);

        using IMessageProducer producer = await session.CreateProducerAsync(destination);
        producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
        ITextMessage sendMessage = await session.CreateTextMessageAsync(message.Content);
        sendMessage.NMSCorrelationID = message.Id.ToString();

        Propagator.Inject(new PropagationContext(contextToInject, Baggage.Current), sendMessage.Properties, InjectTraceContext);
        AddMessageBrokerTags(activity);

        await producer.SendAsync(sendMessage);
    }

    private void AddMessageBrokerTags(Activity? activity)
    {
        activity?.SetTag("messaging.system", "activemq");
        activity?.SetTag("messaging.destination_kind", "queue");
        activity?.SetTag("messaging.destination", queueName);
        activity?.SetTag("messaging.activemq_customTag", "test123");
    }

    private void InjectTraceContext(IPrimitiveMap messageProperties, string key, string value)
    {
        messageProperties.SetString(key, value);
    }
}
