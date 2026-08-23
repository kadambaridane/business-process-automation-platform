using System.ComponentModel.DataAnnotations;

namespace BusinessProcessAutomation.Api.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    public string UsernameOrEmail { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
