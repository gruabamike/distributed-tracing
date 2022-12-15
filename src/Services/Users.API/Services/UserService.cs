using DistributedTracingDotNet.Services.Users.Api.Data;
using DistributedTracingDotNet.Services.Users.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DistributedTracingDotNet.Services.Users.Api.Services;

public class UserService : IUserService
{
    private readonly DataContext context;

    public UserService(DataContext context)
    {
        this.context = context;
    }

    public async Task<List<User>> AddUserAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return await context.Users.ToListAsync();
    }

    public async Task<User> GetAsync(int id) => await context.Users.FindAsync(id);

    public async Task<IList<User>> GetAllAsync() => await context.Users.ToListAsync();
}
