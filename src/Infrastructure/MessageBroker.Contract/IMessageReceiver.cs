namespace MessageBroker.Contract;

public interface IMessageReceiver : IDisposable
{
    Task StartReceiveAsync();
}
