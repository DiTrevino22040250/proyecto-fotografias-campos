using FotografiasCampos.Api.Domain.POCOs;
using Microsoft.EntityFrameworkCore;

namespace FotografiasCampos.Api.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
}
