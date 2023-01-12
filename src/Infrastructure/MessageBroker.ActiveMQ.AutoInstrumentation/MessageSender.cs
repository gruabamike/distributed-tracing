using Apache.NMS;
using Apache.NMS.Util;
using MessageBroker.Contract;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

public class MessageSender : IMessageSender
{
    private static readonly ActiveMQDiagnosticListener diagnosticListener = new(ActiveMQDiagnosticListenerExtensions.DiagnosticListenerName);

    private readonly IActiveMQConnection activeMQConnection;

    public MessageSender(IActiveMQConnection activeMQConnection)
    {
        this.activeMQConnection = activeMQConnection ?? throw new ArgumentNullException(nameof(activeMQConnection));
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

        using ISession session = await connection!.CreateSessionAsync(AcknowledgementMode.AutoAcknowledge);
        using IDestination destination = SessionUtil.GetDestination(session, destinationName);

        using IMessageProducer messageProducer = await session.CreateProducerAsync(destination);
        messageProducer.DeliveryMode = MsgDeliveryMode.NonPersistent;
        Apache.NMS.IMessage sendMessage = await session.CreateMessageAsync();
        sendMessage.NMSMessageId = message.Id.ToString();
        sendMessage.NMSCorrelationID = message.CorrelationId.ToString();
        sendMessage.NMSDestination = destination;

        Exception? exception = default;
        try
        {
            diagnosticListener.WriteMessageSendBefore(activeMQConnection.ConnectionFactory, activeMQConnection.Connection, sendMessage);
            await messageProducer.SendAsync(sendMessage);
            diagnosticListener.WriteMessageSendAfter(activeMQConnection.ConnectionFactory, activeMQConnection.Connection, sendMessage);
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
                diagnosticListener.WriteMessageSendError(activeMQConnection.ConnectionFactory, activeMQConnection.Connection, sendMessage, exception);
            }
        }
    }
}
