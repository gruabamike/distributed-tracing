using Apache.NMS;
using Apache.NMS.ActiveMQ;

namespace MessageBroker.ActiveMQ.ManualInstrumentation;

public class ActiveMQConnection : IActiveMQConnection, IDisposable
{
    public IConnectionFactory ConnectionFactory { get => connectionFactory; }
    public IConnection? Connection { get => connection; }

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
        connection = await connectionFactory.CreateConnectionAsync();
        await connection.StartAsync();
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
        if (!disposed)
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
