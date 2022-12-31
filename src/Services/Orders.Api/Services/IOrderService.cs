using Orders.Api.Commands;
using OrderService.Api.Models;

namespace OrderService.Api.Services;

public interface IOrderService
{
    Task<Order?> GetOrderAsync(Guid orderId);

    Task<IEnumerable<Order>> GetOrdersAsync();

    Task<Order?> AddOrderAsync(CreateOrderCommand createOrderCommand);
}
