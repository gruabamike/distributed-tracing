using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

internal static class ActiveMQDiagnosticListenerExtensions
{
    public const string DiagnosticListenerName = "ActiveMQDiagnosticListener";

    private const string ActiveMQEventNamePrefix = "MessageBroker.ActiveMQ.AutoInstrumentation";

    public const string BeforeConnectionStart = ActiveMQEventNamePrefix + nameof(WriteConnectionStartBefore);
    public const string AfterConnectionStart = ActiveMQEventNamePrefix + nameof(WriteConnectionStartAfter);
    public const string ErrorConnectionStart = ActiveMQEventNamePrefix + nameof(WriteConnectionStartError);

    public const string BeforeMessageSend = ActiveMQEventNamePrefix + nameof(WriteMessageSendBefore);
    public const string AfterMessageSend = ActiveMQEventNamePrefix + nameof(WriteMessageSendAfter);
    public const string ErrorMessageSend = ActiveMQEventNamePrefix + nameof(WriteMessageSendError);

    public const string BeforeMessageReceive = ActiveMQEventNamePrefix + nameof(WriteMessageReceiveBefore);
    public const string AfterMessageReceive = ActiveMQEventNamePrefix + nameof(WriteMessageReceiveAfter);
    public const string ErrorMessageReceive = ActiveMQEventNamePrefix + nameof(WriteMessageReceiveError);

    public static Guid WriteConnectionStartBefore(
        this ActiveMQDiagnosticListener diagnosticListener,
        IConnectionFactory connectionFactory,
        IConnection connection)
        => WriteEvent(
            diagnosticListener,
            BeforeConnectionStart,
            new
            {
                BrokerUri = connectionFactory?.BrokerUri,
                ClientId = connection?.ClientId,
                ConnectionFactory = connectionFactory,
                Connection = connection
            });

    public static Guid WriteConnectionStartAfter(
        this ActiveMQDiagnosticListener diagnosticListener,
        IConnectionFactory connectionFactory,
        IConnection connection)
        => WriteEvent(
            diagnosticListener,
            AfterConnectionStart,
            new
            {
                BrokerUri = connectionFactory?.BrokerUri,
                ClientId = connection?.ClientId,
                ConnectionFactory = connectionFactory,
                Connection = connection
            });

    public static Guid WriteConnectionStartError(
        this ActiveMQDiagnosticListener diagnosticListener,
        IConnectionFactory connectionFactory,
        IConnection connection,
        Exception exception)
        => WriteEvent(
            diagnosticListener,
            ErrorConnectionStart,
            new
            {
                BrokerUri = connectionFactory?.BrokerUri,
                ClientId = connection?.ClientId,
                ConnectionFactory = connectionFactory,
                Connection = connection,
                Exception = exception
            });

    public static Guid WriteMessageSendBefore(
        this ActiveMQDiagnosticListener diagnosticListener,
        IDestination destination,
        IMessageProducer messageProducer,
        IMessage message)
        => WriteEvent(
            diagnosticListener,
            BeforeMessageSend,
            new
            {
                Destination = destination,
                MessageProducer = messageProducer,
                Message = message,
                Queue = destination as IQueue,
                Topic = destination as ITopic
            });

    public static Guid WriteMessageSendAfter(
        this ActiveMQDiagnosticListener diagnosticListener,
        IDestination destination,
        IMessageProducer messageProducer,
        IMessage message)
        => WriteEvent(
            diagnosticListener,
            AfterMessageSend,
            new
            {
                Destination = destination,
                MessageProducer = messageProducer,
                Message = message,
                Queue = destination as IQueue,
                Topic = destination as ITopic
            });

    public static Guid WriteMessageSendError(
        this ActiveMQDiagnosticListener diagnosticListener,
        IDestination destination,
        IMessageProducer messageProducer,
        IMessage message,
        Exception exception)
        => WriteEvent(
            diagnosticListener,
            ErrorMessageSend,
            new
            {
                Destination = destination,
                MessageProducer = messageProducer,
                Message = message,
                Queue = destination as IQueue,
                Topic = destination as ITopic,
                Exception = exception
            });
    public static Guid WriteMessageReceiveBefore(
        this ActiveMQDiagnosticListener diagnosticListener,
        IDestination destination,
        IMessageConsumer messageConsumer,
        IMessage message)
        => WriteEvent(
            diagnosticListener,
            BeforeMessageReceive,
            new
            {
                Destination = destination,
                MessageConsumer = messageConsumer,
                Message = message,
                Queue = destination as IQueue,
                Topic = destination as ITopic
            });

    public static Guid WriteMessageReceiveAfter(
        this ActiveMQDiagnosticListener diagnosticListener,
        IDestination destination,
        IMessageConsumer messageConsumer,
        IMessage message)
        => WriteEvent(
            diagnosticListener,
            AfterMessageReceive,
            new
            {
                Destination = destination,
                MessageConsumer = messageConsumer,
                Message = message,
                Queue = destination as IQueue,
                Topic = destination as ITopic
            });

    public static Guid WriteMessageReceiveError(
        this ActiveMQDiagnosticListener diagnosticListener,
        IDestination destination,
        IMessageConsumer messageConsumer,
        IMessage message,
        Exception exception)
        => WriteEvent(
            diagnosticListener,
            ErrorMessageReceive,
            new
            {
                Destination = destination,
                MessageConsumer = messageConsumer,
                Message = message,
                Queue = destination as IQueue,
                Topic = destination as ITopic,
                Exception = exception
            });

    private static Guid WriteEvent(ActiveMQDiagnosticListener diagnosticListener, string name, object? payload)
    {
        if (diagnosticListener.IsEnabled(name))
        {
            Guid operationId = Guid.NewGuid();

            diagnosticListener.Write(
                name,
                payload);

            return operationId;
        }
        else
        {
            return Guid.Empty;
        }
    }
}
