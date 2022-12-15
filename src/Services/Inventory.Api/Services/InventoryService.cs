using Inventory.Api.Models;

namespace Inventory.Api.Services;

public class InventoryService : IInventoryService
{
    public async Task<Stock> GetByProductId(Guid productId)
    {
        return await Task.FromResult(new Stock(Guid.NewGuid(), 10));
    }
}
