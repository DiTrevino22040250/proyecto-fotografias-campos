using FotografiasCampos.Api.Domain.DTOs.Request;
using FotografiasCampos.Api.Domain.DTOs.Response;

namespace FotografiasCampos.Api.Application.Interfaces;

public interface IPedidoService
{
    Task<IEnumerable<PedidoResponseDto>> ObtenerTodosAsync();
    Task<PedidoResponseDto?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<PedidoResponseDto>> ObtenerPorUsernameAsync(string username);
    Task<PedidoResponseDto> CrearAsync(PedidoRequestDto dto, string username);
    Task<bool> ActualizarAsync(int id, PedidoRequestDto dto);
    Task<bool> EliminarAsync(int id);
}