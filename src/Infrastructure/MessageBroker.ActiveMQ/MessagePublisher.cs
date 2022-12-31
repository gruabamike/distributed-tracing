using Apache.NMS.ActiveMQ;
using Apache.NMS;
using MessageBroker.Contract;
using System.Diagnostics;

namespace MessageBroker.ActiveMQ;

public class MessagePublisher : Contract.IMessageSender
{
    private const string ServiceName = $"{nameof(MessageBroker.ActiveMQ)}.{nameof(MessagePublisher)}";
    private const string ServiceVersion = "1.0.0";
    private const string PublishActivityName = "Publish";

    private static readonly Uri MessageBrokerConnectionUri = new Uri("activemq:tcp://localhost:61616");
    private static readonly ActivitySource ActivitySource = new ActivitySource(ServiceName, ServiceVersion);

    private readonly string queueName;

    public MessagePublisher(string queueName)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException($"'{nameof(queueName)}' cannot be null or whitespace.", nameof(queueName));
        }

        this.queueName = queueName;
    }

    public async Task SendAsync(Contract.IMessage message)
    {
        using var activity = ActivitySource.StartActivity(PublishActivityName, ActivityKind.Producer);

        ActivityContext contextToInject = default;
        if (activity is not null)
        {
            contextToInject = activity.Context;
        }
        else if (Activity.Current is not null)
        {
            contextToInject = Activity.Current.Context;
        }

        //IConnectionFactory connectionFactory = new ConnectionFactory(MessageBrokerConnectionUri);
        //using IConnection connection = await connectionFactory.CreateConnectionAsync();
        //await connection.StartAsync();
        //using ISession session = await connection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        //using IDestination destination = await session.GetQueueAsync(queueName);

        //using Apache.NMS.IMessageProducer producer = await session.CreateProducerAsync(destination);
        //producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
        //ITextMessage sendMessage = await session.CreateTextMessageAsync(message.Content);
        //sendMessage.NMSCorrelationID = message.Id.ToString();

        //await producer.SendAsync(sendMessage);
    }
}
