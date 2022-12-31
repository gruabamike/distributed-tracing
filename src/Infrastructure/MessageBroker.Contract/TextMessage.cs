namespace MessageBroker.Contract;

public class TextMessage : IMessage
{
    public TextMessage(string content)
    {
        Id = Guid.NewGuid();
        Content = content;
    }

    public Guid Id { get; init; }

    public string Content { get; init; }
}
