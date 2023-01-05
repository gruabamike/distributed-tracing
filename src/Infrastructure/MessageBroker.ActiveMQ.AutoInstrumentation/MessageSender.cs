using System.Diagnostics;
using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

public class MessageSender : Contract.IMessageSender
{
    private static readonly ActiveMQDiagnosticListener diagnosticListener = new(ActiveMQDiagnosticListenerExtensions.DiagnosticListenerName);

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
        connectionFactory = new ConnectionFactory(brokerUri);
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

        Exception? exception = default;
        IConnection? connection = default;
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

        using ISession session = await connection.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        using IDestination destination = await session.GetQueueAsync(queueName);

        using IMessageProducer messageProducer = await session.CreateProducerAsync(destination);
        messageProducer.DeliveryMode = MsgDeliveryMode.NonPersistent;
        ITextMessage sendMessage = await session.CreateTextMessageAsync(message.Content);
        sendMessage.NMSCorrelationID = message.Id.ToString();

        contextPropagationHandler.Inject(contextToInject, sendMessage);
        ActiveMQActivityTagHelper.AddMessageBrokerTags(
            activity: activity,
            messageSystem: ActiveMQSourceInfoProvider.ApacheActiveMQSystemName,
            destinationKind: "queue",
            destination: queueName);

        exception = default;
        try
        {
            diagnosticListener.WriteMessageSendBefore(destination, messageProducer, sendMessage);
            await messageProducer.SendAsync(sendMessage);
            diagnosticListener.WriteMessageSendAfter(destination, messageProducer, sendMessage);
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
                diagnosticListener.WriteMessageSendError(destination, messageProducer, sendMessage, exception);
            }
        }

        connection?.Dispose(); // TODO
    }
}
