using DistributedTracingDotNet.Services.Users.API.Models;

namespace DistributedTracingDotNet.Services.Users.API.Services;

public interface IUserService
{
    Task<IList<User>> GetAllAsync();

    Task<User> GetAsync(int id);

    Task<List<User>> AddUserAsync(User user);
}
