namespace MessageBroker.Contract;

public interface IMessageReceiver : IDisposable
{
    Task StartReceiveQueueAsync(string queueName, Func<Contract.IMessage, Task> messageHandler);

    Task StartReceiveTopicAsync(string queueName, Func<Contract.IMessage, Task> messageHandler);
}
