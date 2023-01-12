namespace MessageBroker.Contract;

public interface IMessageSender
{
    Task SendQueueAsync(string queueName, Contract.IMessage message);

    Task SendTopicAsync(string topicName, Contract.IMessage message);
}
