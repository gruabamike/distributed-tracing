using System.Diagnostics;
using Apache.NMS;
using Apache.NMS.Util;

namespace MessageBroker.ActiveMQ.ManualInstrumentation;

public class MessageSender : Contract.IMessageSender
{
    private readonly IActiveMQConnection activeMQConnection;
    private readonly IActiveMQContextPropagationHandler contextPropagationHandler;

    public MessageSender(
        IActiveMQConnection activeMQConnection,
        IActiveMQContextPropagationHandler contextPropagationHandler)
    {
        this.activeMQConnection = activeMQConnection ?? throw new ArgumentNullException(nameof(activeMQConnection));
        this.contextPropagationHandler = contextPropagationHandler ?? throw new ArgumentNullException(nameof(contextPropagationHandler));
    }

    public Task SendQueueAsync(string queueName, Contract.IMessage message)
        => SendAsync($"queue://{queueName}", message);

    public Task SendTopicAsync(string topicName, Contract.IMessage message)
        => SendAsync($"topic://{topicName}", message);

    private async Task SendAsync(string destinationName, Contract.IMessage message)
    {
        IConnection? connection = activeMQConnection.Connection;
        bool isConnectionEstablished = connection?.IsStarted ?? false;
        if (!isConnectionEstablished)
        {
            throw new NMSConnectionException("Connection has not been initialized and started");
        }

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

        using ISession session = await connection!.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        using IDestination destination = SessionUtil.GetDestination(session, destinationName);

        using IMessageProducer producer = await session.CreateProducerAsync(destination);
        producer.DeliveryMode = MsgDeliveryMode.NonPersistent;
        ITextMessage sendMessage = await session.CreateTextMessageAsync(message.Content);
        sendMessage.NMSCorrelationID = message.Id.ToString();
        sendMessage.NMSCorrelationID = message.CorrelationId.ToString();
        sendMessage.NMSDestination = destination;

        contextPropagationHandler.Inject(contextToInject, sendMessage);

        if (activity is not null)
        {
            ActiveMQActivityTagHelper.SetMessagingActivityDetails(
                activity,
                activeMQConnection.ConnectionFactory,
                sendMessage);
        }

        await producer.SendAsync(sendMessage);
    }
}
