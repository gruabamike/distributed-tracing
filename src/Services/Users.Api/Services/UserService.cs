using DistributedTracingDotNet.Services.Users.Api.Data;
using DistributedTracingDotNet.Services.Users.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DistributedTracingDotNet.Services.Users.Api.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> logger;
    private readonly DataContext context;

    public UserService(
        ILogger<UserService> logger,
        DataContext context)
    {
        this.logger = logger;
        this.context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
        => await context.Users.ToListAsync();

    public async Task<User?> GetAsync(Guid id)
        => await context.Users.FindAsync(id);


    public async Task<User?> AddAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return await context.Users.FindAsync(user.Id);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        var existingUser = await context.Users.FindAsync(user.Id);
        if (existingUser is null)
        {
            return false;
        }

        // TODO: Update
        //existingUser.FirstName = user.FirstName;
        //existingUser.LastName = user.LastName;
        await context.SaveChangesAsync();

        return true;
    }
}
