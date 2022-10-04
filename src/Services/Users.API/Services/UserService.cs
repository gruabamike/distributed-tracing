using DistributedTracingDotNet.Services.Users.API.Models;

namespace DistributedTracingDotNet.Services.Users.API.Services;

public class UserService : IUserService
{
    private static List<User> users = new List<User>
    {
        new User(1, "Michael", "Gruber"),
        new User(2, "Lukas", "Seyr")
    };

    public async Task<List<User>> AddUserAsync(User user)
    {
        users.Add(user);
        return users;
    }

    public async Task<User> GetAsync(int id)
    {
        return users.FirstOrDefault(x => x.Id == id);
    }

    public async Task<IList<User>> GetAllAsync()
    {
        return users;
    }
}
