using Apache.NMS.ActiveMQ;
using Apache.NMS;
using System.Diagnostics;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

public class MessageReceiver : Contract.IMessageReceiver, IDisposable
{
    private static readonly ActiveMQDiagnosticListener diagnosticListener = new(ActiveMQDiagnosticListenerExtensions.DiagnosticListenerName);

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

        connectionFactory = new ConnectionFactory(brokerUri);
        this.queueName = queueName;
        this.messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
    }

    public async Task StartReceiveAsync()
    {
        Exception? exception = default;

        try
        {
            connection = await connectionFactory.CreateConnectionAsync();
            diagnosticListener.WriteConnectionStartBefore(connectionFactory, connection);
            await connection.StartAsync();
            diagnosticListener.WriteConnectionStartAfter(connectionFactory, connection);
        }
        catch (Exception e)
        {
            exception = e;
            throw;
        }
        finally
        {
            if (exception is not null)
            {
                diagnosticListener.WriteConnectionStartError(connectionFactory, connection, exception);
            }
        }

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

        Exception? exception = default;

        try
        {
            diagnosticListener.WriteMessageReceiveBefore(null, null, message); // TODO
            await messageHandler(new Contract.TextMessage(message?.ToString() ?? string.Empty));
            diagnosticListener.WriteMessageReceiveAfter(null, null, message); // TODO
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
                diagnosticListener.WriteMessageReceiveError(null, null, message, exception); // TODO
            }
        }
    }

    public void Dispose()
    {
        messageConsumer?.Dispose();
        destination?.Dispose();
        session?.Dispose();
        connection?.Dispose();
    }
}
