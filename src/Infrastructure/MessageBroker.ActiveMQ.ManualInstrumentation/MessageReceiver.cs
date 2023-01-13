using Apache.NMS;
using System.Diagnostics;
using Apache.NMS.Util;
using OpenTelemetry.Trace;

namespace MessageBroker.ActiveMQ.ManualInstrumentation;

public class MessageReceiver : Contract.IMessageReceiver, IDisposable
{
    private readonly IActiveMQContextPropagationHandler contextPropagationHandler;
    private readonly IActiveMQConnection activeMQConnection;

    private ISession? session;
    private IDestination? destination;
    private IMessageConsumer? messageConsumer;
    private Func<Contract.IMessage, Task>? messageHandler;
    private bool disposed = false;

    public MessageReceiver(
        IActiveMQContextPropagationHandler contextPropagationHandler,
        IActiveMQConnection activeMQConnection)
    {
        this.contextPropagationHandler = contextPropagationHandler ?? throw new ArgumentNullException(nameof(contextPropagationHandler));
        this.activeMQConnection = activeMQConnection ?? throw new ArgumentNullException(nameof(activeMQConnection));
    }

    public Task StartReceiveQueueAsync(string queueName, Func<Contract.IMessage, Task> messageHandler)
        => StartReceiveAsync($"queue://{queueName}", messageHandler);

    public Task StartReceiveTopicAsync(string topicName, Func<Contract.IMessage, Task> messageHandler)
        => StartReceiveAsync($"topic://{topicName}", messageHandler);

    private async Task StartReceiveAsync(string destinationName, Func<Contract.IMessage, Task> messageHandler)
    {
        if (string.IsNullOrWhiteSpace(destinationName))
        {
            throw new ArgumentException($"'{nameof(destinationName)}' cannot be null or whitespace.", nameof(destinationName));
        }

        this.messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));

        IConnection? connection = activeMQConnection.Connection;
        bool isConnectionEstablished = connection?.IsStarted ?? false;
        if (!isConnectionEstablished)
        {
            throw new NMSConnectionException("Connection has not been initialized");
        }

        session = await connection!.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        destination = SessionUtil.GetDestination(session, destinationName);
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

        if (activity is not null)
        {
            ActiveMQActivityTagHelper.SetMessagingActivityDetails(
                activity,
                activeMQConnection.ConnectionFactory,
                message);

            activity.SetTag(TraceSemanticConventions.AttributeMessagingOperation, TraceSemanticConventions.MessagingOperationValues.Receive);
            activity.SetTag(TraceSemanticConventions.AttributeMessagingConsumerId, activeMQConnection.Connection?.ClientId ?? "unknown");
        }

        await messageHandler!(new Contract.TextMessage(message?.ToString() ?? string.Empty));
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposed)
        {
            if (disposing)
            {
                DisposeManagedResources();
            }

            disposed = true;
        }
    }

    private void DisposeManagedResources()
    {
        messageConsumer?.Dispose();
        destination?.Dispose();
        session?.Dispose();
    }

    ~MessageReceiver()
    {
        Dispose(disposing: false);
    }
}
