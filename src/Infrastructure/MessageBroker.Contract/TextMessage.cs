namespace MessageBroker.Contract;

public class TextMessage : IMessage
{
    public TextMessage()
    {
        Id = Guid.NewGuid();
    }

    public TextMessage(string content)
        : this()
    {
        Content = content;
    }

    public TextMessage(Guid correlationId, string content)
        : this(content)
    {
        CorrelationId = correlationId;
    }

    public Guid Id { get; init; }

    public Guid CorrelationId { get; set; }

    public string Content { get; init; } = string.Empty;
}
