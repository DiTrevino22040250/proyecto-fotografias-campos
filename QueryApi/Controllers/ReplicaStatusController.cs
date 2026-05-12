using Microsoft.AspNetCore.Mvc;
using QueryApi.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace QueryApi.Controllers;

[ApiController]
[Route("query/[controller]")]
public class ReplicaStatusController : ControllerBase
{
    private readonly ReplicaDbContext _context;
    private readonly ILogger<ReplicaStatusController> _logger;

    public ReplicaStatusController(ReplicaDbContext context, ILogger<ReplicaStatusController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerEstado()
    {
        try
        {
            await _context.Database.ExecuteSqlRawAsync("SELECT 1");
            _logger.LogInformation("✅ Réplica activa y respondiendo correctamente");
            return Ok(new
            {
                estado = "activa",
                mensaje = "La réplica está sincronizando correctamente",
                timestamp = DateTime.Now
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("❌ Réplica no disponible: {Error}", ex.Message);
            return StatusCode(503, new
            {
                estado = "inactiva",
                mensaje = "La réplica no está disponible en este momento",
                timestamp = DateTime.Now
            });
        }
    }
}


