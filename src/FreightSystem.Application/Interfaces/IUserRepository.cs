using FreightSystem.Core.Entities;

namespace FreightSystem.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByIdAsync(int id);
        Task<IEnumerable<string>> GetUserRolesAsync(int userId);
        Task AddUserAsync(User user, IEnumerable<string> roles);
    }
}
