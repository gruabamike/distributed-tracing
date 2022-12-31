using Apache.NMS.ActiveMQ;
using Apache.NMS;

namespace MessageBroker.ActiveMQ.ManualInstrumentation;

public class ActiveMQConnection : Contract.IConnection, IAsyncDisposable
{
    private readonly IConnectionFactory connectionFactory;
    //private readonly IConnection connection;
    //private readonly ISession session;
    //private readonly IDestination destination;
    //private bool isDisposed = false;

    public ActiveMQConnection(
        IConnectionFactory connectionFactory)
    {
        this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

    }

    public Task<Contract.IMessageReceiver> CreateQueueMessageReceiver()
    {
        throw new NotImplementedException();
    }

    public Task<Contract.IMessageSender> CreateQueueMessageSender()
    {
        throw new NotImplementedException();
    }

    public Task<Contract.IMessageReceiver> CreateTopicMessageReceiver()
    {
        throw new NotImplementedException();
    }

    public Task<Contract.IMessageSender> CreateTopicMessageSender()
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

}
