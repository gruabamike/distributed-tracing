namespace Fulfillment;

public class FulfillmentManager : IFulfillmentManager
{
    public async Task Fulfill()
    {
        await Task.Delay(200);
    }
}