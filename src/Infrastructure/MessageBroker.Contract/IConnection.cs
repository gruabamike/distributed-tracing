namespace MessageBroker.Contract;

public interface IConnection
{
    Task<IMessageSender> CreateQueueMessageSender();

    Task<IMessageReceiver> CreateQueueMessageReceiver();

    Task<IMessageSender> CreateTopicMessageSender();

    Task<IMessageReceiver> CreateTopicMessageReceiver();
}
