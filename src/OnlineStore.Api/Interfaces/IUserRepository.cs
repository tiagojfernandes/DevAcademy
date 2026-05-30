using OnlineStore.Api.Entities;

namespace OnlineStore.Api.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user);
    Task<IReadOnlyList<string>> GetRolesAsync(int userId);
    Task AssignRoleAsync(int userId, string roleName);
}
