using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Entities;
using FreightSystem.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FreightSystem.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly FreightDbContext _db;

        public UserRepository(FreightDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _db.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId)
        {
            return await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
        }

        public async Task AddUserAsync(User user, IEnumerable<string> roles)
        {
            _db.Users.Add(user);
            foreach (var roleName in roles)
            {
                var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
                if (role == null)
                {
                    role = new Role { Name = roleName };
                    _db.Roles.Add(role);
                }

                _db.UserRoles.Add(new UserRole { User = user, Role = role });
            }

            await _db.SaveChangesAsync();
        }
    }
}
