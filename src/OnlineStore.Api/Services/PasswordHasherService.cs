using Microsoft.AspNetCore.Identity;
using OnlineStore.Api.Entities;
using OnlineStore.Api.Interfaces;

namespace OnlineStore.Api.Services;

public class PasswordHasherService : IPasswordHasher
{
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password) =>
        _hasher.HashPassword(null!, password);

    public bool Verify(string password, string hashedPassword) =>
        _hasher.VerifyHashedPassword(null!, hashedPassword, password) != PasswordVerificationResult.Failed;
}
