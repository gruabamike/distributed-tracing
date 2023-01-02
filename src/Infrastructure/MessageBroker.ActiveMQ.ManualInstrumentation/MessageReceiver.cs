using Apache.NMS.ActiveMQ;
using Apache.NMS;
using System.Diagnostics;

namespace MessageBroker.ActiveMQ.ManualInstrumentation;

public class MessageReceiver : Contract.IMessageReceiver, IDisposable
{
    private readonly IActiveMQContextPropagationHandler contextPropagationHandler;
    private readonly string queueName;
    private readonly Func<Contract.IMessage, Task> messageHandler;

    private readonly IConnectionFactory connectionFactory;
    private IConnection? connection;
    private ISession? session;
    private IDestination? destination;
    private IMessageConsumer? messageConsumer;

    public MessageReceiver(
        IActiveMQContextPropagationHandler contextPropagationHandler,
        Uri brokerUri,
        string queueName,
        Func<Contract.IMessage, Task> messageHandler)
    {
        this.contextPropagationHandler = contextPropagationHandler ?? throw new ArgumentNullException(nameof(contextPropagationHandler));
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException($"'{nameof(queueName)}' cannot be null or whitespace.", nameof(queueName));
        }

        this.connectionFactory = new ConnectionFactory(brokerUri);
        this.queueName = queueName;
        this.messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
    }

    public async Task StartReceiveAsync()
    {
        connection = await connectionFactory.CreateConnectionAsync();
        await connection.StartAsync();
        session = await connection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        destination = await session.GetQueueAsync(queueName);

        messageConsumer = await session.CreateConsumerAsync(destination);
        messageConsumer.Listener += OnMessageReceived;
    }

    private async void OnMessageReceived(IMessage message)
    {
        var parentContext = contextPropagationHandler.Extract(message);

        using var activity = ActiveMQSourceInfoProvider.ActivitySource.StartActivity(
            ActiveMQSourceInfoProvider.ReceiveMessageActivityName,
            ActivityKind.Consumer,
            parentContext.ActivityContext,
            ActiveMQSourceInfoProvider.ActivityCreationTags!);

        ActiveMQActivityTagHelper.AddMessageBrokerTags(
            activity: activity,
            messageSystem: ActiveMQSourceInfoProvider.ApacheActiveMQSystemName,
            destinationKind: "queue",
            destination: queueName);
        await messageHandler(new Contract.TextMessage(message?.ToString() ?? string.Empty));
    }

    public void Dispose()
    {
        messageConsumer?.Dispose();
        destination?.Dispose();
        session?.Dispose();
        connection?.Dispose();
    }
}
