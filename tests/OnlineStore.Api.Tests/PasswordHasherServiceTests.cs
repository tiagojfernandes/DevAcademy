using OnlineStore.Api.Services;

namespace OnlineStore.Api.Tests;

public class PasswordHasherServiceTests
{
    [Fact]
    public void Verify_ReturnsTrue_ForCorrectPassword()
    {
        var hasher = new PasswordHasherService();
        var hash = hasher.Hash("Passw0rd");

        Assert.True(hasher.Verify("Passw0rd", hash));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var hasher = new PasswordHasherService();
        var hash = hasher.Hash("Passw0rd");

        Assert.False(hasher.Verify("wrong", hash));
    }

    [Fact]
    public void Hash_ProducesDifferentOutput_EachCall()
    {
        var hasher = new PasswordHasherService();

        // Identity hasher uses a random salt, so the same password hashes differently each time.
        Assert.NotEqual(hasher.Hash("Passw0rd"), hasher.Hash("Passw0rd"));
    }
}
