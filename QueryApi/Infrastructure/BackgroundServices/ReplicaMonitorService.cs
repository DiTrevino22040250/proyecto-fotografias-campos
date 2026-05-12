using Microsoft.EntityFrameworkCore;
using QueryApi.Infrastructure.Data;

namespace QueryApi.Infrastructure.BackgroundServices;

public class ReplicaMonitorService : BackgroundService
{
    private readonly ILogger<ReplicaMonitorService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _intervalo = TimeSpan.FromMinutes(1);

    public ReplicaMonitorService(
        ILogger<ReplicaMonitorService> logger,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🔄 Monitor de réplica iniciado - verificando cada minuto");

        while (!stoppingToken.IsCancellationRequested)
        {
            await VerificarReplica();
            await Task.Delay(_intervalo, stoppingToken);
        }
    }

    private async Task VerificarReplica()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ReplicaDbContext>();
            await context.Database.ExecuteSqlRawAsync("SELECT 1");
            _logger.LogInformation("✅ [{Timestamp}] Réplica ACTIVA - sincronización correcta", DateTime.Now);
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ [{Timestamp}] Réplica INACTIVA - {Error}", DateTime.Now, ex.Message);
        }
    }
}


