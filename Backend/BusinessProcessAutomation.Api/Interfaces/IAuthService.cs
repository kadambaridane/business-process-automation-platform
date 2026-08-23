using BusinessProcessAutomation.Api.DTOs.Auth;

namespace BusinessProcessAutomation.Api.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}
