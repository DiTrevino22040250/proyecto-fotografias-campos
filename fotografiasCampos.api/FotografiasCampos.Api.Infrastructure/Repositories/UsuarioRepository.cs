using FotografiasCampos.Api.Domain.Interfaces;
using FotografiasCampos.Api.Domain.POCOs;
using FotografiasCampos.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FotografiasCampos.Api.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> ObtenerPorUsernameAsync(string username) =>
        await _context.Usuarios.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> CrearAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        return await _context.SaveChangesAsync() > 0;
    }
}