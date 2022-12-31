using Apache.NMS.ActiveMQ;
using MessageBroker.Contract;

namespace MessageBroker.ActiveMQ.ManualInstrumentation;

public class ActiveMQConnectionFactory : Contract.IConnectionFactory
{
    public IConnection CreateConnection(Uri messageBrokerUri)
    {
        return new ActiveMQConnection(new ConnectionFactory(messageBrokerUri));
    }
}
