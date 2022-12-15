using DistributedTracingDotNet.Services.Users.Api.Models;

namespace DistributedTracingDotNet.Services.Users.Api.Services;

public interface IUserService
{
    Task<IList<User>> GetAllAsync();

    Task<User> GetAsync(int id);

    Task<List<User>> AddUserAsync(User user);
}
