using FotografiasCampos.Api.Application.Interfaces;
using FotografiasCampos.Api.Domain.DTOs.Request;
using FotografiasCampos.Api.Domain.Interfaces;

namespace FotografiasCampos.Api.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepo;
    private readonly IJwtService _jwtService;

    public AuthService(IUsuarioRepository usuarioRepo, IJwtService jwtService)
    {
        _usuarioRepo = usuarioRepo;
        _jwtService = jwtService;
    }

    public async Task<string?> LoginAsync(LoginRequestDto dto)
    {
        var usuario = await _usuarioRepo.ObtenerPorUsernameAsync(dto.Username);
        if (usuario == null) return null;

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, usuario.PasswordHash))
            return null;

        return _jwtService.GenerarToken(usuario);
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto dto)
    {
        var existe = await _usuarioRepo.ObtenerPorUsernameAsync(dto.Username);
        if (existe != null) return false;

        var usuario = new FotografiasCampos.Api.Domain.POCOs.Usuario
        {
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            NombreCompleto = dto.NombreCompleto,
            Email = dto.Email,
            Telefono = dto.Telefono,
            Rol = "cliente"
        };

        return await _usuarioRepo.CrearAsync(usuario);
    }
}