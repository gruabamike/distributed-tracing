using MessageBroker.Contract;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Commands;
using Orders.Api.Data;
using Orders.Api.Models;

namespace Orders.Api.Services;

public class OrderService : IOrderService
{
    private readonly ILogger<OrderService> logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IMessageSender messageSender;
    private readonly DataContext context;

    public OrderService(
        ILogger<OrderService> logger,
        IHttpClientFactory httpClientFactory,
        IMessageSender messageSender,
        DataContext context)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        this.messageSender = messageSender ?? throw new ArgumentNullException(nameof(messageSender));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Order>> GetOrdersAsync()
        => await context.Orders.ToListAsync();

    public async Task<Order?> GetOrderAsync(Guid orderId)
        => await context.Orders.FindAsync(orderId);

    public async Task<Order?> AddOrderAsync(CreateOrderCommand createOrderCommand)
    {
        var (userId, productId, quantity) = (createOrderCommand.UserId, createOrderCommand.ProductId, createOrderCommand.Quantity);

        if (!await UserExists(userId))
        {
            logger.LogWarning($"Could not create order. User '{userId}' does not exist.");
            return null;
        }

        if (!await IsInventoryAvailable(productId, quantity))
        {
            logger.LogWarning($"Could not create order. No stock for product '{productId}' with quantity '{quantity}' available.");
        }

        var order = new Order(Guid.NewGuid());

        logger.LogInformation($"Adding order with id: {order.Id}");
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();
        logger.LogInformation($"Order has been successfully created.");

        await messageSender.SendAsync(new TextMessage(order.Id.ToString()));

        return await context.Orders.FindAsync(order.Id);
    }

    private async Task<bool> UserExists(Guid userId)
    {
        var httpClient = httpClientFactory.CreateClient("Users");
        var httpResponseMessage = await httpClient.GetAsync($"users/{userId}");

        return httpResponseMessage.IsSuccessStatusCode;
    }

    private async Task<bool> IsInventoryAvailable(Guid productId, int quantity)
    {
        var httpClient = httpClientFactory.CreateClient("Inventory");
        var httpResponseMessage = await httpClient.GetAsync($"inventory/{productId}");//await httpClient.PostAsJsonAsync($"inventory/{productId}", new { ProductId = productId, Quantity = quantity });

        return httpResponseMessage.IsSuccessStatusCode;
    }
}
