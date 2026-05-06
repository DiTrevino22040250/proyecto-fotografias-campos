using FotografiasCampos.Api.Domain.Interfaces;
using FotografiasCampos.Api.Domain.POCOs;
using FotografiasCampos.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FotografiasCampos.Api.Infrastructure.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly AppDbContext _context;

    public PedidoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Pedido>> ObtenerTodosAsync() =>
        await _context.Pedidos.Include(p => p.Usuario).ToListAsync();

    public async Task<Pedido?> ObtenerPorIdAsync(int id) =>
        await _context.Pedidos.Include(p => p.Usuario).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<Pedido>> ObtenerPorUsernameAsync(string username) =>
        await _context.Pedidos
            .Include(p => p.Usuario)
            .Where(p => p.Usuario!.Username == username)
            .ToListAsync();

    public async Task<Pedido> CrearAsync(Pedido pedido)
    {
        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();
        return await ObtenerPorIdAsync(pedido.Id) ?? pedido;
    }

    public async Task<bool> ActualizarAsync(Pedido pedido)
    {
        _context.Pedidos.Update(pedido);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null) return false;
        _context.Pedidos.Remove(pedido);
        return await _context.SaveChangesAsync() > 0;
    }
}