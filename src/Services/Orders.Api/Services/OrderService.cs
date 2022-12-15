using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Models;

namespace OrderService.Api.Services;

public class OrderService : IOrderService
{
    private readonly DataContext context;

    public OrderService(DataContext context)
    {
        this.context = context;
    }

    public async Task<List<Order>> AddOrderAsync(Order order)
    {
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        return await context.Orders.ToListAsync();
    }
}
