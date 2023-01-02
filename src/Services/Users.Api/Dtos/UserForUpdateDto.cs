namespace DistributedTracingDotNet.Services.Users.Api.Dtos;

public record UserForUpdateDto(Guid UserId, string FirstName, string LastName);
