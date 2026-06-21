using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class ResetPasswordDto
{
    public string Email { get; set; } = "";
    public string Token { get; set; } = null!;
    public string NewPassword { get; set; } = null!;

    [Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = null!;
}
