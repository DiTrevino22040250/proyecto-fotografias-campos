using FotografiasCampos.Api.Domain.DTOs.Request;

namespace FotografiasCampos.Api.Application.Interfaces;

public interface IAuthService
{
    Task<string?> LoginAsync(LoginRequestDto dto);
    Task<bool> RegisterAsync(RegisterRequestDto dto);
}