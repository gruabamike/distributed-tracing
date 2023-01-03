using System.Diagnostics;
using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace MessageBroker.ActiveMQ.ManualInstrumentation;

public class MessageSender : Contract.IMessageSender
{
    private readonly IActiveMQContextPropagationHandler contextPropagationHandler;
    private readonly string queueName;
    private readonly IConnectionFactory connectionFactory;

    public MessageSender(
        IActiveMQContextPropagationHandler contextPropagationHandler,
        Uri brokerUri,
        string queueName)
    {
        this.contextPropagationHandler = contextPropagationHandler ?? throw new ArgumentNullException(nameof(contextPropagationHandler));
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException($"'{nameof(queueName)}' cannot be null or whitespace.", nameof(queueName));
        }

        this.queueName = queueName;
        this.connectionFactory = new ConnectionFactory(brokerUri);
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

        using IConnection connection = await connectionFactory.CreateConnectionAsync();
        await connection.StartAsync();
        using ISession session = await connection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        using IDestination destination = await session.GetQueueAsync(queueName);

        using IMessageProducer producer = await session.CreateProducerAsync(destination);
        producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
        ITextMessage sendMessage = await session.CreateTextMessageAsync(message.Content);
        sendMessage.NMSCorrelationID = message.Id.ToString();

        contextPropagationHandler.Inject(contextToInject, sendMessage);
        ActiveMQActivityTagHelper.AddMessageBrokerTags(
            activity: activity,
            messageSystem: ActiveMQSourceInfoProvider.ApacheActiveMQSystemName,
            destinationKind: "queue",
            destination: queueName);

        await producer.SendAsync(sendMessage);
    }
}
