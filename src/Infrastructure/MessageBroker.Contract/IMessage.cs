namespace MessageBroker.Contract;

public interface IMessage
{
    Guid Id { get; }
    Guid CorrelationId { get; }
    string Content { get; }
}
