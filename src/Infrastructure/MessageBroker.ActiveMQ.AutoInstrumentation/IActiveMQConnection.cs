using Apache.NMS;

namespace MessageBroker.ActiveMQ.AutoInstrumentation;

public interface IActiveMQConnection : Contract.IBrokerConnection
{
    IConnectionFactory ConnectionFactory { get; }

    IConnection? Connection { get; }
}
