namespace Orders.Api.Dtos;

public record OrderForCreationDto(Guid UserId, Guid ProductId, int Quantity);
