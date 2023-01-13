using Apache.NMS;

namespace MessageBroker.ActiveMQ.ManualInstrumentation;

public interface IActiveMQConnection : Contract.IBrokerConnection
{
    IConnectionFactory ConnectionFactory { get; }

    IConnection? Connection { get; }
}
