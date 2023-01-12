using Apache.NMS;
using Apache.NMS.Util;
using MessageBroker.Contract;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

public class MessageReceiver : IMessageReceiver, IDisposable
{
    private static readonly ActiveMQDiagnosticListener diagnosticListener = new(ActiveMQDiagnosticListenerExtensions.DiagnosticListenerName);

    private readonly IActiveMQConnection activeMQConnection;

    private ISession? session;
    private IDestination? destination;
    private IMessageConsumer? messageConsumer;
    private Func<Contract.IMessage, Task>? messageHandler;
    private bool disposed = false;

    public MessageReceiver(IActiveMQConnection activeMQConnection)
    {
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

    private async void OnMessageReceived(Apache.NMS.IMessage message)
    {
        Exception? exception = default;

        try
        {
            diagnosticListener.WriteMessageReceiveBefore(activeMQConnection.ConnectionFactory, activeMQConnection.Connection, message);
            await messageHandler!(new Contract.TextMessage(message.ToString()!));
            diagnosticListener.WriteMessageReceiveAfter(activeMQConnection.ConnectionFactory, activeMQConnection.Connection, message);
        }
        catch(Exception e)
        {
            exception = e;
            throw;
        }
        finally
        {
            if (exception is not null)
            {
                diagnosticListener.WriteMessageReceiveError(activeMQConnection.ConnectionFactory, activeMQConnection.Connection, message, exception);
            }
        }
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
