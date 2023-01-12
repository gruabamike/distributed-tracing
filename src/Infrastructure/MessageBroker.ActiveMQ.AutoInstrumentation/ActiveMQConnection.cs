using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

public class ActiveMQConnection : IActiveMQConnection, IDisposable
{
    public IConnectionFactory ConnectionFactory { get => this.connectionFactory; }
    public IConnection? Connection { get => this.connection; }

    private static readonly ActiveMQDiagnosticListener diagnosticListener = new(ActiveMQDiagnosticListenerExtensions.DiagnosticListenerName);

    private readonly IConnectionFactory connectionFactory;
    private IConnection? connection;
    private bool disposed = false;

    public ActiveMQConnection(Uri brokerUri, string clientName)
	{
        if (string.IsNullOrWhiteSpace(clientName))
        {
            throw new ArgumentException($"'{nameof(clientName)}' cannot be null or whitespace.", nameof(clientName));
        }

        connectionFactory = new ConnectionFactory(brokerUri, ActiveMQClientIdProvider.GetClientId(clientName));
    }

    public async Task OpenAsync()
    {
        Exception? exception = default;

        try
        {
            diagnosticListener.WriteConnectionStartBefore(connectionFactory, connection);
            connection = await connectionFactory.CreateConnectionAsync();
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
    }

    public async Task CloseAsync()
    {
        bool isConnectionEstablished = connection?.IsStarted ?? false;
        if (!isConnectionEstablished)
        {
            throw new NMSConnectionException("Connection has not been initialized and started");
        }

        await connection!.CloseAsync();
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
        connection?.Dispose();
    }

    ~ActiveMQConnection()
    {
        Dispose(disposing: false);
    }
}
