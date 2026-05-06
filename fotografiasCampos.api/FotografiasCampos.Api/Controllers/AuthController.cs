using FotografiasCampos.Api.Application.Interfaces;
using FotografiasCampos.Api.Domain.DTOs.Request;
using Microsoft.AspNetCore.Mvc;

namespace FotografiasCampos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var token = await _authService.LoginAsync(dto);
        if (token == null)
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos" });

        return Ok(new { token });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
    {
        var resultado = await _authService.RegisterAsync(dto);
        if (!resultado)
            return BadRequest(new { mensaje = "El usuario ya existe" });

        return Ok(new { mensaje = "Usuario registrado correctamente" });
    }
}