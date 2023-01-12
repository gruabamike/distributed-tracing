namespace MessageBroker.Contract;

public interface IBrokerConnection
{
    Task OpenAsync();

    Task CloseAsync();
}
