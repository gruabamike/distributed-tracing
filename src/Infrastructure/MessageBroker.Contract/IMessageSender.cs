namespace MessageBroker.Contract;

public interface IMessageSender
{
    Task SendAsync(IMessage message);
}
