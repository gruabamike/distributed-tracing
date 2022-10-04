using DistributedTracingDotNet.Services.Users.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DistributedTracingDotNet.Services.Users.API.Data
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
                new User { Id = 1, FirstName = "Khadeeja", LastName = "Glenn" },
                new User { Id = 2, FirstName = "Monty", LastName = "Vang" },
                new User { Id = 3, FirstName = "Roseanne", LastName = "Hodges" },
                new User { Id = 4, FirstName = "Antoine", LastName = "Bellamy" },
                new User { Id = 5, FirstName = "Felicia", LastName = "Dowling" },
                new User { Id = 6, FirstName = "Sidrah", LastName = "Humphries" },
                new User { Id = 7, FirstName = "Carly", LastName = "Haas" },
                new User { Id = 8, FirstName = "Isadora", LastName = "Greig" },
                new User { Id = 9, FirstName = "Cadi", LastName = "Bull" },
                new User { Id = 10, FirstName = "Jaxon", LastName = "Gentry" }
            );
        }
    }
}