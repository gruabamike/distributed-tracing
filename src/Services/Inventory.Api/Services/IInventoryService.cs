using Inventory.Api.Models;

namespace Inventory.Api.Services;

public interface IInventoryService
{
    Task<Stock> GetByProductId(Guid productId);
}
