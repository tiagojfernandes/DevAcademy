using OnlineStore.Api.Common;
using OnlineStore.Api.DTOs;
using OnlineStore.Api.Events;
using OnlineStore.Api.Interfaces;

namespace OnlineStore.Api.Services;

public class AuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IPasswordHasher _hasher;
    private readonly IEventBus _eventBus;
    private readonly JwtTokenService _jwt;

    public AuthService(
        IUserRepository userRepo,
        IPasswordHasher hasher,
        IEventBus eventBus,
        JwtTokenService jwt)
    {
        _userRepo = userRepo;
        _hasher   = hasher;
        _eventBus = eventBus;
        _jwt      = jwt;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(string username, string password, CancellationToken ct = default)
    {
        var existing = await _userRepo.GetByUsernameAsync(username);
        if (existing is not null)
            return Result<AuthResponse>.Conflict("Username already exists.");

        var user = new Entities.User
        {
            Username     = username,
            Password     = _hasher.Hash(password),
            LastModified = DateTime.UtcNow
        };

        var created = await _userRepo.CreateAsync(user);
        await _userRepo.AssignRoleAsync(created.Id, "User");

        _eventBus.Publish(new UserRegisteredEvent(created.Id, DateTimeOffset.UtcNow));

        var roles = await _userRepo.GetRolesAsync(created.Id);
        var token = _jwt.CreateToken(created, roles);
        return Result<AuthResponse>.Ok(new AuthResponse(created.Id, created.Username, token, roles));
    }

    // Returns null when the credentials are invalid, endpoint returns 401.
    public async Task<AuthResponse?> LoginAsync(string username, string password, CancellationToken ct = default)
    {
        var user = await _userRepo.GetByUsernameAsync(username);
        if (user is null || !_hasher.Verify(password, user.Password))
            return null;

        var roles = await _userRepo.GetRolesAsync(user.Id);
        var token = _jwt.CreateToken(user, roles);
        return new AuthResponse(user.Id, user.Username, token, roles);
    }
}

