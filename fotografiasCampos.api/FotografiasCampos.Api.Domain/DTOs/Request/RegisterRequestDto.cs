 using System.ComponentModel.DataAnnotations;

namespace FotografiasCampos.Api.Domain.DTOs.Request;

public class RegisterRequestDto
{
    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Telefono { get; set; } = string.Empty;
}
