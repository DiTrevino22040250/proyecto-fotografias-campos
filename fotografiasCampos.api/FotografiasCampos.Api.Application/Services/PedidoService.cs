using FotografiasCampos.Api.Application.Interfaces;
using FotografiasCampos.Api.Domain.DTOs.Request;
using FotografiasCampos.Api.Domain.DTOs.Response;
using FotografiasCampos.Api.Domain.Interfaces;
using FotografiasCampos.Api.Domain.POCOs;

namespace FotografiasCampos.Api.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _repo;
    private readonly IUsuarioRepository _usuarioRepo;

    public PedidoService(IPedidoRepository repo, IUsuarioRepository usuarioRepo)
    {
        _repo = repo;
        _usuarioRepo = usuarioRepo;
    }

    public async Task<IEnumerable<PedidoResponseDto>> ObtenerTodosAsync()
    {
        var pedidos = await _repo.ObtenerTodosAsync();
        return pedidos.Select(MapToDto);
    }

    public async Task<PedidoResponseDto?> ObtenerPorIdAsync(int id)
    {
        var p = await _repo.ObtenerPorIdAsync(id);
        return p == null ? null : MapToDto(p);
    }

    public async Task<IEnumerable<PedidoResponseDto>> ObtenerPorUsernameAsync(string username)
    {
        var pedidos = await _repo.ObtenerPorUsernameAsync(username);
        return pedidos.Select(MapToDto);
    }

    public async Task<PedidoResponseDto> CrearAsync(PedidoRequestDto dto, string username)
    {
        if (dto.FechaEntrega.DayOfWeek == DayOfWeek.Sunday)
            throw new InvalidOperationException("La fecha de entrega no puede ser domingo.");

        var usuario = await _usuarioRepo.ObtenerPorUsernameAsync(username);
        if (usuario == null)
            throw new InvalidOperationException("Usuario no encontrado.");

        var pedido = new Pedido
        {
            NombreCliente = dto.NombreCliente,
            Telefono = dto.Telefono,
            TipoServicio = dto.TipoServicio,
            FechaEntrega = dto.FechaEntrega,
            CantidadFotos = dto.CantidadFotos,
            Precio = dto.Precio,
            UsuarioId = usuario.Id
        };

        var creado = await _repo.CrearAsync(pedido);
        return MapToDto(creado);
    }

    public async Task<bool> ActualizarAsync(int id, PedidoRequestDto dto)
    {
        var pedido = await _repo.ObtenerPorIdAsync(id);
        if (pedido == null) return false;

        if (dto.FechaEntrega.DayOfWeek == DayOfWeek.Sunday)
            throw new InvalidOperationException("La fecha de entrega no puede ser domingo.");

        pedido.NombreCliente = dto.NombreCliente;
        pedido.Telefono = dto.Telefono;
        pedido.TipoServicio = dto.TipoServicio;
        pedido.FechaEntrega = dto.FechaEntrega;
        pedido.CantidadFotos = dto.CantidadFotos;
        pedido.Precio = dto.Precio;

        return await _repo.ActualizarAsync(pedido);
    }

    public async Task<bool> EliminarAsync(int id) =>
        await _repo.EliminarAsync(id);

    private static PedidoResponseDto MapToDto(Pedido p) => new()
    {
        Id = p.Id,
        NombreCliente = p.NombreCliente,
        TipoServicio = p.TipoServicio,
        FechaEntrega = p.FechaEntrega,
        CantidadFotos = p.CantidadFotos,
        Precio = p.Precio,
        Username = p.Usuario?.Username ?? string.Empty
    };
}