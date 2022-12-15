using Microsoft.EntityFrameworkCore;
using OrderService.Api.Models;

namespace OrderService.Api.Data;

internal class OrderDbInitializer
{
    private readonly ModelBuilder modelBuilder;

    public OrderDbInitializer(ModelBuilder modelBuilder)
    {
        this.modelBuilder = modelBuilder;
    }

    public void Seed()
    {
        modelBuilder.Entity<Order>().HasData(
            new Order(Guid.Parse("o9fd2671-6386-41da-bb51-f04d3e772f91"))
        );
    }
}
