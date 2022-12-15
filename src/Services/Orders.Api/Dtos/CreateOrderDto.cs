namespace OrderService.Api.Dtos;

public record CreateOrderDto(Guid UserId, Guid ProductId, int Quantity);
