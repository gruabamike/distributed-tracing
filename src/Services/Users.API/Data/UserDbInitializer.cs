using DistributedTracingDotNet.Services.Users.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DistributedTracingDotNet.Services.Users.Api.Data
{
    internal class UserDbInitializer
    {
        private readonly ModelBuilder modelBuilder;

        public UserDbInitializer(ModelBuilder modelBuilder)
        {
            this.modelBuilder = modelBuilder;
        }

        public void Seed()
        {
            modelBuilder.Entity<User>().HasData(
                new User(Guid.Parse("00000000-0000-0000-0000-000000000001"), "Khadeeja", "Glenn"),
                new User(Guid.Parse("00000000-0000-0000-0000-000000000002"), "Monty", "Vang"),
                new User(Guid.Parse("00000000-0000-0000-0000-000000000003"), "Roseanne", "Hodges"),
                new User(Guid.Parse("00000000-0000-0000-0000-000000000004"), "Antoine", "Bellamy"),
                new User(Guid.Parse("00000000-0000-0000-0000-000000000005"), "Felicia", "Dowling"),
                new User(Guid.Parse("00000000-0000-0000-0000-000000000006"), "Sidrah", "Humphries"),
                new User(Guid.Parse("00000000-0000-0000-0000-000000000007"), "Carly", "Haas"),
                new User(Guid.Parse("00000000-0000-0000-0000-000000000008"), "Isadora", "Greig"),
                new User(Guid.Parse("00000000-0000-0000-0000-000000000009"), "Cadi", "Bull"),
                new User(Guid.Parse("00000000-0000-0000-0000-000000000010"), "Jaxon", "Gentry")
            );
        }
    }
}