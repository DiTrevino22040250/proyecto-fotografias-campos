using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace FotografiasCampos.Api.Infrastructure.Middleware;

public class SecurityLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityLoggingMiddleware> _logger;

    public SecurityLoggingMiddleware(RequestDelegate next, ILogger<SecurityLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var method = context.Request.Method;
        var path = context.Request.Path;
        var userAgent = context.Request.Headers["User-Agent"].ToString();

        _logger.LogInformation(
            "📥 REQUEST | IP: {IP} | Method: {Method} | Path: {Path} | UserAgent: {UserAgent}",
            ip, method, path, userAgent);

        await _next(context);

        var statusCode = context.Response.StatusCode;

        if (statusCode == 400)
            _logger.LogWarning("⚠️ BAD REQUEST | IP: {IP} | Path: {Path} | Posible ataque detectado", ip, path);

        if (statusCode == 401)
            _logger.LogWarning("🔒 UNAUTHORIZED | IP: {IP} | Path: {Path} | Intento sin autenticación", ip, path);

        if (statusCode == 429)
            _logger.LogError("🚨 RATE LIMIT | IP: {IP} | Path: {Path} | Posible ataque DDoS o fuerza bruta", ip, path);
    }
}