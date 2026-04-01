using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Infrastructure.Persistence;
using FreightSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Cryptography;
using System.Text;

namespace FreightSystem.Infrastructure.Persistence
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FreightDbContext>();
            var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            await context.Database.MigrateAsync();

            // Ensure roles exist via EF seed and migration.
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Name = "Admin" },
                    new Role { Name = "Operation" },
                    new Role { Name = "Sales" },
                    new Role { Name = "Accountant" }
                );
                await context.SaveChangesAsync();
            }

            // Seed default users only once
            if (!context.Users.Any())
            {
                await AddDefaultUser(userRepo, "admin", "Admin123!", new[] { "Admin" });
                await AddDefaultUser(userRepo, "operation", "Op123!", new[] { "Operation" });
                await AddDefaultUser(userRepo, "sales", "Sales123!", new[] { "Sales" });
                await AddDefaultUser(userRepo, "accountant", "Ac123!", new[] { "Accountant" });
            }
        }

        private static async Task AddDefaultUser(IUserRepository userRepo, string username, string password, IEnumerable<string> roles)
        {
            var existing = await userRepo.GetByUsernameAsync(username);
            if (existing != null) return;

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                IsActive = true
            };
            await userRepo.AddUserAsync(user, roles);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyPassword(string plain, string hash)
        {
            return HashPassword(plain) == hash;
        }
    }
}
