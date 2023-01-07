using Apache.NMS;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

internal static class ActiveMQDiagnosticListenerExtensions
{
    public const string DiagnosticListenerName = "ActiveMQDiagnosticListener";

    private const string ActiveMQEventNamePrefix = "MessageBroker.ActiveMQ.AutoInstrumentation.";

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
        IConnectionFactory? connectionFactory,
        IConnection? connection)
        => WriteEvent(
            diagnosticListener,
            BeforeConnectionStart,
            new
            {
                ConnectionFactory = connectionFactory,
                Connection = connection
            });

    public static Guid WriteConnectionStartAfter(
        this ActiveMQDiagnosticListener diagnosticListener,
        IConnectionFactory? connectionFactory,
        IConnection? connection)
        => WriteEvent(
            diagnosticListener,
            AfterConnectionStart,
            new
            {
                ConnectionFactory = connectionFactory,
                Connection = connection
            });

    public static Guid WriteConnectionStartError(
        this ActiveMQDiagnosticListener diagnosticListener,
        IConnectionFactory? connectionFactory,
        IConnection? connection,
        Exception exception)
        => WriteEvent(
            diagnosticListener,
            ErrorConnectionStart,
            new
            {
                ConnectionFactory = connectionFactory,
                Connection = connection,
                Exception = exception
            });

    public static Guid WriteMessageSendBefore(
        this ActiveMQDiagnosticListener diagnosticListener,
        IConnectionFactory? connectionFactory,
        IConnection? connection,
        IMessage message)
        => WriteEvent(
            diagnosticListener,
            BeforeMessageSend,
            new
            {
                ConnectionFactory = connectionFactory,
                Connection = connection,
                Message = message,
            });

    public static Guid WriteMessageSendAfter(
        this ActiveMQDiagnosticListener diagnosticListener,
        IConnectionFactory? connectionFactory,
        IConnection? connection,
        IMessage message)
        => WriteEvent(
            diagnosticListener,
            AfterMessageSend,
            new
            {
                ConnectionFactory = connectionFactory,
                Connection = connection,
                Message = message,
            });

    public static Guid WriteMessageSendError(
        this ActiveMQDiagnosticListener diagnosticListener,
        IConnectionFactory? connectionFactory,
        IConnection? connection,
        IMessage message,
        Exception exception)
        => WriteEvent(
            diagnosticListener,
            ErrorMessageSend,
            new
            {
                ConnectionFactory = connectionFactory,
                Connection = connection,
                Message = message,
                Exception = exception
            });

    public static Guid WriteMessageReceiveBefore(
        this ActiveMQDiagnosticListener diagnosticListener,
        IConnectionFactory? connectionFactory,
        IConnection? connection,
        IMessage message)
        => WriteEvent(
            diagnosticListener,
            BeforeMessageReceive,
            new
            {
                ConnectionFactory = connectionFactory,
                Connection = connection,
                Message = message,
            });

    public static Guid WriteMessageReceiveAfter(
        this ActiveMQDiagnosticListener diagnosticListener,
        IConnectionFactory? connectionFactory,
        IConnection? connection,
        IMessage message)
        => WriteEvent(
            diagnosticListener,
            AfterMessageReceive,
            new
            {
                ConnectionFactory = connectionFactory,
                Connection = connection,
                Message = message,
            });

    public static Guid WriteMessageReceiveError(
        this ActiveMQDiagnosticListener diagnosticListener,
        IConnectionFactory? connectionFactory,
        IConnection? connection,
        IMessage message,
        Exception exception)
        => WriteEvent(
            diagnosticListener,
            ErrorMessageReceive,
            new
            {
                ConnectionFactory = connectionFactory,
                Connection = connection,
                Message = message,
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
