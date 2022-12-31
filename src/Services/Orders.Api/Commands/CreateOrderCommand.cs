namespace Orders.Api.Commands;

public record CreateOrderCommand(Guid UserId, Guid ProductId, int Quantity);
