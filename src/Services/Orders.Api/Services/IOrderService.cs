using Orders.Api.Commands;
using Orders.Api.Models;

namespace Orders.Api.Services;

public interface IOrderService
{
    Task<Order?> GetOrderAsync(Guid orderId);

    Task<IEnumerable<Order>> GetOrdersAsync();

    Task<Order?> AddOrderAsync(CreateOrderCommand createOrderCommand);
}
