using QueryApi.Application.DTOs;

namespace QueryApi.Application.Interfaces;

public interface IPedidoQueryService
{
    Task<IEnumerable<PedidoUnificadoDto>> ObtenerTodosAsync(string username, string rol);
    Task<PedidoUnificadoDto?> ObtenerPorIdAsync(int id, string username, string rol);
    Task<IEnumerable<PedidoUnificadoDto>> ObtenerPorUsernameAsync(string username, string rol);
    Task<IEnumerable<PedidoUnificadoDto>> ObtenerPorEstadoAsync(string estado);
} 
