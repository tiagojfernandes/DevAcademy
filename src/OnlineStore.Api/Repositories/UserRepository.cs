using Microsoft.EntityFrameworkCore;
using OnlineStore.Api.Data;
using OnlineStore.Api.Entities;
using OnlineStore.Api.Interfaces;

namespace OnlineStore.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(int id) =>
        await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User> CreateAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<IReadOnlyList<string>> GetRolesAsync(int userId) =>
        await _db.UserRoles.Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name).ToListAsync();

    public async Task AssignRoleAsync(int userId, string roleName)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role is null)
        {
            role = new Role { Name = roleName };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();
        }

        if (!await _db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id))
        {
            _db.UserRoles.Add(new UserRole { UserId = userId, RoleId = role.Id });
            await _db.SaveChangesAsync();
        }
    }
}
