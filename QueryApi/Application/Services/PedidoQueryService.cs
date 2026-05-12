using QueryApi.Application.DTOs;
using QueryApi.Application.Interfaces;
using QueryApi.Domain.Interfaces;

namespace QueryApi.Application.Services;

public class PedidoQueryService : IPedidoQueryService
{
    private readonly IPedidoQueryRepository _repo;

    public PedidoQueryService(IPedidoQueryRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<PedidoUnificadoDto>> ObtenerTodosAsync(string username, string rol)
    {
        if (rol == "admin")
        {
            var todos = await _repo.ObtenerTodosAsync();
            return todos.Select(MapToDto);
        }

        var misPedidos = await _repo.ObtenerPorUsernameAsync(username);
        return misPedidos.Select(MapToDto);
    }

    public async Task<PedidoUnificadoDto?> ObtenerPorIdAsync(int id, string username, string rol)
    {
        var pedido = await _repo.ObtenerPorIdAsync(id);
        if (pedido == null) return null;

        if (rol != "admin" && pedido.Username != username)
            return null;

        return MapToDto(pedido);
    }

    public async Task<IEnumerable<PedidoUnificadoDto>> ObtenerPorUsernameAsync(string username, string rol)
    {
        if (rol != "admin" && rol != username)
        {
            var propios = await _repo.ObtenerPorUsernameAsync(username);
            return propios.Select(MapToDto);
        }

        var pedidos = await _repo.ObtenerPorUsernameAsync(username);
        return pedidos.Select(MapToDto);
    }

    public async Task<IEnumerable<PedidoUnificadoDto>> ObtenerPorEstadoAsync(string estado)
    {
        var pedidos = await _repo.ObtenerPorEstadoAsync(estado);
        return pedidos.Select(MapToDto);
    }

    private static PedidoUnificadoDto MapToDto(Domain.Entities.PedidoUnificado p) => new()
    {
        Id = p.Id,
        NombreCliente = p.NombreCliente,
        TipoServicio = p.TipoServicio,
        FechaEntrega = p.FechaEntrega,
        CantidadFotos = p.CantidadFotos,
        Precio = p.Precio,
        Estado = p.Estado,
        Username = p.Username,
        Email = p.Email,
        FechaCreacion = p.FechaCreacion
    };
}


