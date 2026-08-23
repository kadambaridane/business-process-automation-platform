using BusinessProcessAutomation.Api.Models;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IEnumerable<string> roles, out DateTime expiresAt);
}
