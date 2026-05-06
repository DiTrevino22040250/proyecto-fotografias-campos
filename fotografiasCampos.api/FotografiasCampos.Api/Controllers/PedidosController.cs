using System.Security.Claims;
using FotografiasCampos.Api.Application.Interfaces;
using FotografiasCampos.Api.Domain.DTOs.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FotografiasCampos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    public PedidosController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var rol = User.FindFirst(ClaimTypes.Role)?.Value;

        if (rol == "admin")
        {
            var todos = await _pedidoService.ObtenerTodosAsync();
            return Ok(todos);
        }

        var misPedidos = await _pedidoService.ObtenerPorUsernameAsync(username!);
        return Ok(misPedidos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var pedido = await _pedidoService.ObtenerPorIdAsync(id);
        if (pedido == null)
            return NotFound(new { mensaje = "Pedido no encontrado" });

        var username = User.FindFirst(ClaimTypes.Name)?.Value;
        var rol = User.FindFirst(ClaimTypes.Role)?.Value;

        if (rol != "admin" && pedido.Username != username)
            return Forbid();

        return Ok(pedido);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] PedidoRequestDto dto)
    {
        try
        {
            var username = User.FindFirst(ClaimTypes.Name)?.Value!;
            var pedido = await _pedidoService.CrearAsync(dto, username);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = pedido.Id }, pedido);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] PedidoRequestDto dto)
    {
        try
        {
            var resultado = await _pedidoService.ActualizarAsync(id, dto);
            if (!resultado)
                return NotFound(new { mensaje = "Pedido no encontrado" });
            return Ok(new { mensaje = "Pedido actualizado correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var resultado = await _pedidoService.EliminarAsync(id);
        if (!resultado)
            return NotFound(new { mensaje = "Pedido no encontrado" });
        return Ok(new { mensaje = "Pedido eliminado correctamente" });
    }
}