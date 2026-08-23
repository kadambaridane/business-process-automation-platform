using BusinessProcessAutomation.Api.Helpers;
using Xunit;

namespace BusinessProcessAutomation.Api.Tests.Auth;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ThenVerify_WithCorrectPassword_ReturnsTrue()
    {
        var hash = _hasher.Hash("CorrectHorseBatteryStaple1!");

        Assert.True(_hasher.Verify("CorrectHorseBatteryStaple1!", hash));
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        var hash = _hasher.Hash("CorrectHorseBatteryStaple1!");

        Assert.False(_hasher.Verify("WrongPassword", hash));
    }

    [Fact]
    public void Hash_NeverReturnsPlainText()
    {
        var hash = _hasher.Hash("CorrectHorseBatteryStaple1!");

        Assert.DoesNotContain("CorrectHorseBatteryStaple1!", hash);
    }
}
