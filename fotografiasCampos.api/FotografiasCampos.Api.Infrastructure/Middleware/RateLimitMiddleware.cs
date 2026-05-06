using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace FotografiasCampos.Api.Infrastructure.Middleware;

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private const int MaxRequestsPerMinute = 200;
    private const int MaxLoginAttemptsPerMinute = 20;

    public RateLimitMiddleware(RequestDelegate next, IMemoryCache cache)
    {
        _next = next;
        _cache = cache;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Usar X-Test-IP si existe (para tests), sino usar IP real
        var ip = context.Request.Headers["X-Test-IP"].FirstOrDefault()
                 ?? context.Connection.RemoteIpAddress?.ToString()
                 ?? "unknown";

        var path = context.Request.Path.Value?.ToLower() ?? "";
        var isLoginEndpoint = path.Contains("auth/login");

        var key = $"ratelimit_{ip}_{(isLoginEndpoint ? "login" : "general")}";
        var limit = isLoginEndpoint ? MaxLoginAttemptsPerMinute : MaxRequestsPerMinute;

        var requestCount = _cache.GetOrCreate(key, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            return 0;
        });

        if (requestCount >= limit)
        {
            context.Response.StatusCode = 429;
            await context.Response.WriteAsJsonAsync(new
            {
                mensaje = isLoginEndpoint
                    ? "Demasiados intentos de login. Espera 1 minuto."
                    : "Demasiadas peticiones. Espera 1 minuto."
            });
            return;
        }

        _cache.Set(key, requestCount + 1, TimeSpan.FromMinutes(1));
        await _next(context);
    }
}