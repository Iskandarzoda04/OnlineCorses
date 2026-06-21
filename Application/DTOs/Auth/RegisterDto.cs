using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Auth;

public class RegisterDto
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Password { get; set; } = null!;
    public string Role { get; set; } = "";

    [Compare("Password")]
    public string ConfirmPassword { get; set; } = null!;
}
