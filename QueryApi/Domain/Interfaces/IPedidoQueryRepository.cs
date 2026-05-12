using QueryApi.Domain.Entities;

namespace QueryApi.Domain.Interfaces;

public interface IPedidoQueryRepository
{
    Task<IEnumerable<PedidoUnificado>> ObtenerTodosAsync();
    Task<PedidoUnificado?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<PedidoUnificado>> ObtenerPorUsernameAsync(string username);
    Task<IEnumerable<PedidoUnificado>> ObtenerPorEstadoAsync(string estado);
}


