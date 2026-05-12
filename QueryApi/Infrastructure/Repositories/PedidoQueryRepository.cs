using Microsoft.EntityFrameworkCore;
using QueryApi.Domain.Entities;
using QueryApi.Domain.Interfaces;
using QueryApi.Infrastructure.Data;

namespace QueryApi.Infrastructure.Repositories;

public class PedidoQueryRepository : IPedidoQueryRepository
{
    private readonly ReplicaDbContext _context;

    public PedidoQueryRepository(ReplicaDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PedidoUnificado>> ObtenerTodosAsync() =>
        await _context.PedidosUnificados.ToListAsync();

    public async Task<PedidoUnificado?> ObtenerPorIdAsync(int id) =>
        await _context.PedidosUnificados
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<PedidoUnificado>> ObtenerPorUsernameAsync(string username) =>
        await _context.PedidosUnificados
            .Where(p => p.Username == username)
            .ToListAsync();

    public async Task<IEnumerable<PedidoUnificado>> ObtenerPorEstadoAsync(string estado) =>
        await _context.PedidosUnificados
            .Where(p => p.Estado == estado)
            .ToListAsync();
}



