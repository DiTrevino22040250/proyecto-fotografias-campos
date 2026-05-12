using Microsoft.EntityFrameworkCore;
using QueryApi.Domain.Entities;

namespace QueryApi.Infrastructure.Data;

public class ReplicaDbContext : DbContext
{
    public ReplicaDbContext(DbContextOptions<ReplicaDbContext> options) : base(options) { }

    public DbSet<PedidoUnificado> PedidosUnificados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PedidoUnificado>(entity =>
        {
            entity.HasNoKey();
            entity.ToView("vista_pedidos_unificados");
        });
    }
}
 
