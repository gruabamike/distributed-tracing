namespace MessageBroker.Contract;

public interface IMessage
{
    Guid Id { get; }
    string Content { get; }
}
