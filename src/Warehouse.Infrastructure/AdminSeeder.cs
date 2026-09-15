using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Warehouse.Domain.Entities;
using Warehouse.Infrastructure.Data;

namespace Warehouse.Infrastructure
{
    public static class AdminSeeder
    {
        public static async Task SeedAsync(
            WarehouseDbContext context,
            IConfiguration configuration,
            IPasswordHasher<User> passwordHasher)
        {
            var adminExists = await context.Users.AnyAsync(x => x.Role == UserRole.Administrator);

            if (adminExists)
                return;

            var password = configuration["Admin:Password"];

            if (string.IsNullOrEmpty(password))
                throw new InvalidOperationException(
                    "Admin password is not configured.");

            var admin = new User
            {
                Username = configuration["Admin:Username"] ?? "Admin",
                Email = "admin@example.com",
                Role = UserRole.Administrator,
                CreatedAt = DateTime.UtcNow
            };

            admin.PasswordHash = passwordHasher.HashPassword(
                admin,
                password);

            context.Users.Add(admin);

            await context.SaveChangesAsync();
        }
    }
}
