using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

public class MessageSender : Contract.IMessageSender
{
    private static readonly ActiveMQDiagnosticListener diagnosticListener = new(ActiveMQDiagnosticListenerExtensions.DiagnosticListenerName);

    private readonly string queueName;
    private readonly IConnectionFactory connectionFactory;

    public MessageSender(
        Uri brokerUri,
        string queueName)
    {
        if (string.IsNullOrWhiteSpace(queueName))
        {
            throw new ArgumentException($"'{nameof(queueName)}' cannot be null or whitespace.", nameof(queueName));
        }

        this.queueName = queueName;
        connectionFactory = new ConnectionFactory(brokerUri);
    }

    public async Task SendAsync(Contract.IMessage message)
    {
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

        exception = default;
        try
        {
            diagnosticListener.WriteMessageSendBefore(null, connection, sendMessage); // TODO
            await messageProducer.SendAsync(sendMessage);
            diagnosticListener.WriteMessageSendAfter(null, connection, sendMessage); // TODO
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
                diagnosticListener.WriteMessageSendError(null, connection, sendMessage, exception); // TODO
            }
        }

        connection?.Dispose(); // TODO
    }
}
