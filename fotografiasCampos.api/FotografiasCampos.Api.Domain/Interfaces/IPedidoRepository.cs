using FotografiasCampos.Api.Domain.POCOs;

namespace FotografiasCampos.Api.Domain.Interfaces;

public interface IPedidoRepository
{
    Task<IEnumerable<Pedido>> ObtenerTodosAsync();
    Task<Pedido?> ObtenerPorIdAsync(int id);
    Task<IEnumerable<Pedido>> ObtenerPorUsernameAsync(string username);
    Task<Pedido> CrearAsync(Pedido pedido);
    Task<bool> ActualizarAsync(Pedido pedido);
    Task<bool> EliminarAsync(int id);
}