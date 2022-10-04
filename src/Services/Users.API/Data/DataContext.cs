using DistributedTracingDotNet.Services.Users.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DistributedTracingDotNet.Services.Users.API.Data;

public class DataContext : DbContext
{
	public DataContext(DbContextOptions<DataContext> options) : base(options) { }

	public DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
		new UserDbInitializer(modelBuilder).Seed();
	}
}
