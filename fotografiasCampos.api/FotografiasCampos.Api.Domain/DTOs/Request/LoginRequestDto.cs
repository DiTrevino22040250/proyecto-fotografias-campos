 using System.ComponentModel.DataAnnotations;

namespace FotografiasCampos.Api.Domain.DTOs.Request;

public class LoginRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
