using Microsoft.AspNetCore.Http;

namespace FotografiasCampos.Api.Infrastructure.Middleware;

public class InputSanitizationMiddleware
{
    private readonly RequestDelegate _next;

    private static readonly string[] PeligrososPatrones =
    {
        "<script", "</script>", "javascript:", "onload=", "onerror=",
        "DROP TABLE", "DROP DATABASE",
        "' OR '1'='1", "' OR 1=1",
        "<iframe", "<object", "<embed"
    };

    // Headers del sistema que debemos ignorar
    private static readonly string[] HeadersPermitidos =
    {
        "content-type", "authorization", "accept", "user-agent",
        "host", "connection", "accept-encoding", "accept-language",
        "postman-token", "cache-control", "content-length"
    };

    public InputSanitizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var paramPeligroso = context.Request.Query
            .Where(param => ContienePeligroso(param.Value.ToString()))
            .FirstOrDefault();

        if (paramPeligroso.Key != null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                mensaje = "Se detectó contenido no permitido en la petición."
            });
            return;
        }

        var headerPeligroso = context.Request.Headers
            .Where(h => !HeadersPermitidos.Contains(h.Key.ToLower()))
            .Where(h => ContienePeligroso(h.Value.ToString()))
            .FirstOrDefault();

        if (headerPeligroso.Key != null)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new
            {
                mensaje = "Se detectó contenido no permitido en los headers."
            });
            return;
        }

        await _next(context);
    }

    private static bool ContienePeligroso(string valor)
    {
        if (string.IsNullOrEmpty(valor)) return false;
        var valorUpper = valor.ToUpper();
        return PeligrososPatrones.Any(p => valorUpper.Contains(p.ToUpper()));
    }
}