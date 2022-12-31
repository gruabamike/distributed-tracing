namespace MessageBroker.Contract;
public interface IConnectionFactory
{
    IConnection CreateConnection(Uri messageBrokerUri);
}
