using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QueryApi.Application.Interfaces;

namespace QueryApi.Controllers;

[ApiController]
[Route("query/[controller]")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly IPedidoQueryService _service;

    public PedidosController(IPedidoQueryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        var rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var pedidos = await _service.ObtenerTodosAsync(username, rol);
        return Ok(pedidos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        var rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
        var pedido = await _service.ObtenerPorIdAsync(id, username, rol);
        if (pedido == null)
            return NotFound(new { mensaje = "Pedido no encontrado" });
        return Ok(pedido);
    }

    [HttpGet("cliente/{username}")]
    public async Task<IActionResult> ObtenerPorUsername(string username)
    {
        var usernameActual = User.FindFirst(ClaimTypes.Name)?.Value ?? "";
        var rol = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

        if (rol != "admin" && usernameActual != username)
            return Forbid();

        var pedidos = await _service.ObtenerPorUsernameAsync(username, rol);
        return Ok(pedidos);
    }

    [HttpGet("estado/{estado}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ObtenerPorEstado(string estado)
    {
        var pedidos = await _service.ObtenerPorEstadoAsync(estado);
        return Ok(pedidos);
    }
}


