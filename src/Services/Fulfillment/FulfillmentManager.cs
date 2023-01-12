namespace Fulfillment;

public class FulfillmentManager : IFulfillmentManager
{
    private const int SIMULATION_PROCESSING_TIME_MS = 150;

    public async Task Fulfill()
    {
        await Task.Delay(SIMULATION_PROCESSING_TIME_MS);
    }
}