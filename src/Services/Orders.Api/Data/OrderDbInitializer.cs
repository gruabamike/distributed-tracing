using Microsoft.EntityFrameworkCore;
using Orders.Api.Models;

namespace Orders.Api.Data;

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
            new Order(Guid.Parse("00000000-0000-0000-0000-000000000001"))
        );
    }
}
