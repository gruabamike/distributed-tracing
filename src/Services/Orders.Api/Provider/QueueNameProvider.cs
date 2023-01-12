namespace Orders.Api.Provider;

public class QueueNameProvider : IQueueNameProvider
{
    private readonly string orderProcessingQueueName;

    public QueueNameProvider(string orderProcessingQueueName)
	{
        if (string.IsNullOrWhiteSpace(orderProcessingQueueName))
        {
            throw new ArgumentException($"'{nameof(orderProcessingQueueName)}' cannot be null or whitespace.", nameof(orderProcessingQueueName));
        }

        this.orderProcessingQueueName = orderProcessingQueueName;
    }

    public string OrderProcessingQueueName => orderProcessingQueueName;
}
