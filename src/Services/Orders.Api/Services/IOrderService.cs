using OrderService.Api.Models;

namespace OrderService.Api.Services;

public interface IOrderService
{
    Task<List<Order>> AddOrderAsync(Order order);
}
