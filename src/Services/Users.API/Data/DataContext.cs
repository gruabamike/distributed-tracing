using DistributedTracingDotNet.Shared.Users.Contract;
using Microsoft.EntityFrameworkCore;

namespace DistributedTracingDotNet.Services.Users.API.Data;

public class DataContext : DbContext
{
	public DataContext(DbContextOptions<DataContext> options) : base(options)
	{

	}

	public DbSet<User> Users { get; set; }
}
