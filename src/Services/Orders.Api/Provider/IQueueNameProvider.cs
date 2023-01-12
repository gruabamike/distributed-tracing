namespace Orders.Api.Provider;

public interface IQueueNameProvider
{
    string OrderProcessingQueueName { get; }
}
