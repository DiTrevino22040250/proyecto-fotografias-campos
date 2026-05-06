using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FotografiasCampos.Api.Domain.DTOs.Request;
using FotografiasCampos.Api.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace FotografiasCampos.Api.Tests.Config;

public class FotografiasCamposWebApplication<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            if (!db.Usuarios.Any(u => u.Username == "admin"))
            {
                db.Usuarios.Add(new FotografiasCampos.Api.Domain.POCOs.Usuario
                {
                    Username = "admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                    Rol = "admin",
                    NombreCompleto = "Administrador",
                    Email = "admin@fotografiascampos.com",
                    Telefono = "0000000000"
                });
                db.SaveChanges();
            }

            if (!db.Usuarios.Any(u => u.Username == "cliente1"))
            {
                db.Usuarios.Add(new FotografiasCampos.Api.Domain.POCOs.Usuario
                {
                    Username = "cliente1",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Rol = "cliente",
                    NombreCompleto = "Juan Pérez",
                    Email = "juan@example.com",
                    Telefono = "6181234567"
                });
                db.SaveChanges();
            }
        });
    }
}

public class IntegrationTests : IClassFixture<FotografiasCamposWebApplication<Program>>
{
    private readonly HttpClient _client;
    private static string? _tokenAdminCache;
    private static string? _tokenClienteCache;
    private static readonly SemaphoreSlim _tokenLock = new SemaphoreSlim(1, 1);

    public IntegrationTests(FotografiasCamposWebApplication<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> ObtenerTokenAdminAsync()
    {
        if (_tokenAdminCache != null) return _tokenAdminCache;
        await _tokenLock.WaitAsync();
        try
        {
            if (_tokenAdminCache != null) return _tokenAdminCache;
            var dto = new LoginRequestDto { Username = "admin", Password = "password" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                throw new Exception("RateLimiter activado. Espera 1 minuto.");
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            _tokenAdminCache = content!["token"];
            return _tokenAdminCache;
        }
        finally { _tokenLock.Release(); }
    }

    private async Task<string> ObtenerTokenClienteAsync()
    {
        if (_tokenClienteCache != null) return _tokenClienteCache;
        await _tokenLock.WaitAsync();
        try
        {
            if (_tokenClienteCache != null) return _tokenClienteCache;
            var dto = new LoginRequestDto { Username = "cliente1", Password = "password123" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
                throw new Exception("RateLimiter activado. Espera 1 minuto.");
            var content = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            _tokenClienteCache = content!["token"];
            return _tokenClienteCache;
        }
        finally { _tokenLock.Release(); }
    }

    private string ObtenerIdDePedido(Dictionary<string, object> pedido)
    {
        var idKey = pedido.Keys.FirstOrDefault(k => k.ToLower() == "id");
        if (idKey == null) throw new Exception("No se encontró el campo 'id' en la respuesta");
        var valor = pedido[idKey];
        if (valor is JsonElement element)
            return element.GetInt32().ToString();
        return valor.ToString()!;
    }

    // ✅ AUTH TESTS
    [Fact]
    public async Task Login_ConCredencialesCorrectas_DebeRetornarToken()
    {
        var dto = new LoginRequestDto { Username = "admin", Password = "password" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("token", content);
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecto_DebeRetornar401()
    {
        var dto = new LoginRequestDto { Username = "admin", Password = "wrongpassword" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ConUsuarioInexistente_DebeRetornar401()
    {
        var dto = new LoginRequestDto { Username = "noexiste", Password = "password" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_SinDatos_DebeRetornar400()
    {
        var dto = new LoginRequestDto { Username = "", Password = "" };
        var response = await _client.PostAsJsonAsync("/api/auth/login", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_GeneraToken_ConClaimsCorrectos()
    {
        var token = await ObtenerTokenAdminAsync();
        Assert.NotNull(token);
        Assert.NotEmpty(token);
        var partes = token.Split('.');
        Assert.Equal(3, partes.Length);
    }

    // ✅ REGISTER TESTS
    [Fact]
    public async Task Register_ConDatosValidos_DebeRetornar200()
    {
        var username = $"clientetest_{Guid.NewGuid().ToString("N")[..8]}";
        var dto = new RegisterRequestDto
        {
            Username = username,
            Password = "password123",
            NombreCompleto = "Cliente Test",
            Email = $"{username}@example.com",
            Telefono = "6181234567"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_ConUsernameExistente_DebeRetornar400()
    {
        var dto = new RegisterRequestDto
        {
            Username = "admin",
            Password = "password123",
            NombreCompleto = "Otro Admin",
            Email = "otro@example.com",
            Telefono = "6181234567"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Register_SinDatos_DebeRetornar400()
    {
        var dto = new RegisterRequestDto();
        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ✅ RATE LIMITER TESTS
    [Fact]
    public async Task Login_MultiplesIntentos_DebeActivarRateLimiter()
    {
        var dto = new LoginRequestDto { Username = "userquenoexiste", Password = "wrongpassword" };
        HttpResponseMessage? lastResponse = null;

        _client.DefaultRequestHeaders.Remove("X-Test-IP");
        _client.DefaultRequestHeaders.Add("X-Test-IP", "test-ratelimit-login");

        for (int i = 0; i < 25; i++)
        {
            lastResponse = await _client.PostAsJsonAsync("/api/auth/login", dto);
            if ((int)lastResponse.StatusCode == 429)
                break;
        }

        _client.DefaultRequestHeaders.Remove("X-Test-IP");
        Assert.Equal(429, (int)lastResponse!.StatusCode);
    }

    [Fact]
    public async Task Peticiones_Generales_MultiplesVeces_DebeActivarRateLimiter()
    {
        HttpResponseMessage? lastResponse = null;

        _client.DefaultRequestHeaders.Remove("X-Test-IP");
        _client.DefaultRequestHeaders.Add("X-Test-IP", "test-ratelimit-general");

        for (int i = 0; i < 205; i++)
        {
            lastResponse = await _client.GetAsync("/api/pedidos");
            if ((int)lastResponse.StatusCode == 429)
                break;
        }

        _client.DefaultRequestHeaders.Remove("X-Test-IP");
        Assert.Equal(429, (int)lastResponse!.StatusCode);
    }

    // ✅ SECURITY HEADERS TESTS
    [Fact]
    public async Task Peticion_DebeIncluirSecurityHeaders()
    {
        var response = await _client.GetAsync("/api/pedidos");
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.True(response.Headers.Contains("X-Frame-Options"));
        Assert.True(response.Headers.Contains("X-XSS-Protection"));
    }

    // ✅ INPUT SANITIZATION TESTS
    [Fact]
    public async Task Peticion_ConScriptEnQuery_DebeRetornar400()
    {
        var response = await _client.GetAsync("/api/pedidos?nombre=<script>alert('xss')</script>");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Peticion_ConSQLInjectionEnQuery_DebeRetornar400()
    {
        var response = await _client.GetAsync("/api/pedidos?nombre=' OR '1'='1");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Peticion_ConIframeEnQuery_DebeRetornar400()
    {
        var response = await _client.GetAsync("/api/pedidos?nombre=<iframe src='malicious.com'>");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Peticion_ConDropTableEnQuery_DebeRetornar400()
    {
        var response = await _client.GetAsync("/api/pedidos?nombre=DROP TABLE Usuarios");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Peticion_ConQueryNormal_DebeNoPasarSanitizacion()
    {
        var response = await _client.GetAsync("/api/pedidos?nombre=Juan");
        Assert.NotEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // ✅ JWT TESTS
    [Fact]
    public async Task Peticion_ConTokenExpirado_DebeRetornar401()
    {
        var tokenExpirado = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsImV4cCI6MTYwMDAwMDAwMH0.invalidsignature";
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokenExpirado);
        var response = await _client.GetAsync("/api/pedidos");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Peticion_ConTokenMalformado_DebeRetornar401()
    {
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "tokenbasura123");
        var response = await _client.GetAsync("/api/pedidos");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ✅ PEDIDOS TESTS - ADMIN
    [Fact]
    public async Task ObtenerPedidos_SinToken_DebeRetornar401()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var response = await _client.GetAsync("/api/pedidos");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ObtenerPedidos_AdminConToken_DebeRetornar200()
    {
        var token = await ObtenerTokenAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/pedidos");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CrearPedido_AdminConDatosValidos_DebeRetornar201()
    {
        var token = await ObtenerTokenAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var dto = new PedidoRequestDto
        {
            NombreCliente = "Juan Pérez",
            Telefono = "6181234567",
            TipoServicio = "Servicio Militar",
            FechaEntrega = DateTime.Now.AddDays(5),
            CantidadFotos = 5,
            Precio = 150.00m
        };

        var response = await _client.PostAsJsonAsync("/api/pedidos", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CrearPedido_ConFechaDomingo_DebeRetornar400()
    {
        var token = await ObtenerTokenAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var domingo = DateTime.Now;
        while (domingo.DayOfWeek != DayOfWeek.Sunday)
            domingo = domingo.AddDays(1);

        var dto = new PedidoRequestDto
        {
            NombreCliente = "Juan Pérez",
            Telefono = "6181234567",
            TipoServicio = "Servicio Militar",
            FechaEntrega = domingo,
            CantidadFotos = 5,
            Precio = 150.00m
        };

        var response = await _client.PostAsJsonAsync("/api/pedidos", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CrearPedido_SinDatos_DebeRetornar400()
    {
        var token = await ObtenerTokenAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var dto = new PedidoRequestDto();
        var response = await _client.PostAsJsonAsync("/api/pedidos", dto);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ObtenerPedidoPorId_Inexistente_DebeRetornar404()
    {
        var token = await ObtenerTokenAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/pedidos/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task EliminarPedido_Inexistente_DebeRetornar404()
    {
        var token = await ObtenerTokenAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.DeleteAsync("/api/pedidos/9999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
public async Task ActualizarPedido_Admin_ConDatosValidos_DebeRetornar200()
{
    var token = await ObtenerTokenAdminAsync();
    _client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    var crearDto = new PedidoRequestDto
    {
        NombreCliente = "Pedro López",
        Telefono = "6181234567",
        TipoServicio = "Trabajo",
        FechaEntrega = DateTime.Now.AddDays(4),
        CantidadFotos = 2,
        Precio = 80.00m
    };

    var crearResponse = await _client.PostAsJsonAsync("/api/pedidos", crearDto);
    var pedidoCreado = await crearResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
    var id = ObtenerIdDePedido(pedidoCreado!);

    // Buscar próximo lunes para garantizar que no sea domingo
    var fechaActualizar = DateTime.Now.AddDays(7);
    while (fechaActualizar.DayOfWeek == DayOfWeek.Sunday)
        fechaActualizar = fechaActualizar.AddDays(1);

    var actualizarDto = new PedidoRequestDto
    {
        NombreCliente = "Pedro López Actualizado",
        Telefono = "6181234567",
        TipoServicio = "Trabajo",
        FechaEntrega = fechaActualizar,
        CantidadFotos = 3,
        Precio = 90.00m
    };

    var response = await _client.PutAsJsonAsync($"/api/pedidos/{id}", actualizarDto);
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
}
    
    [Fact]
public async Task CrearPedido_DebeRetornarPedidoConUsername()
{
    var token = await ObtenerTokenAdminAsync();
    _client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    var dto = new PedidoRequestDto
    {
        NombreCliente = "Test Username",
        Telefono = "6181234567",
        TipoServicio = "Servicio Militar",
        FechaEntrega = DateTime.Now.AddDays(5),
        CantidadFotos = 5,
        Precio = 150.00m
    };

    var response = await _client.PostAsJsonAsync("/api/pedidos", dto);
    var content = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.True(content!.ContainsKey("username"));
}

    [Fact]
    public async Task EliminarPedido_Admin_Existente_DebeRetornar200()
    {
        var token = await ObtenerTokenAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var crearDto = new PedidoRequestDto
        {
            NombreCliente = "Para Eliminar",
            Telefono = "6181234567",
            TipoServicio = "Servicio Militar",
            FechaEntrega = DateTime.Now.AddDays(4),
            CantidadFotos = 1,
            Precio = 50.00m
        };

        var crearResponse = await _client.PostAsJsonAsync("/api/pedidos", crearDto);
        var pedidoCreado = await crearResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var id = ObtenerIdDePedido(pedidoCreado!);

        var response = await _client.DeleteAsync($"/api/pedidos/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ObtenerPedidoPorId_Existente_DebeRetornar200()
    {
        var token = await ObtenerTokenAdminAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var crearDto = new PedidoRequestDto
        {
            NombreCliente = "Test GetById",
            Telefono = "6181234567",
            TipoServicio = "Universidad",
            FechaEntrega = DateTime.Now.AddDays(4),
            CantidadFotos = 2,
            Precio = 70.00m
        };

        var crearResponse = await _client.PostAsJsonAsync("/api/pedidos", crearDto);
        var pedidoCreado = await crearResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var id = ObtenerIdDePedido(pedidoCreado!);

        var response = await _client.GetAsync($"/api/pedidos/{id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // ✅ PEDIDOS TESTS - CLIENTE
    [Fact]
    public async Task ObtenerPedidos_ClienteConToken_DebeRetornar200()
    {
        var token = await ObtenerTokenClienteAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/api/pedidos");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CrearPedido_ClienteConDatosValidos_DebeRetornar201()
    {
        var token = await ObtenerTokenClienteAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var dto = new PedidoRequestDto
        {
            NombreCliente = "Juan Pérez",
            Telefono = "6181234567",
            TipoServicio = "Titulo Universidad",
            FechaEntrega = DateTime.Now.AddDays(3),
            CantidadFotos = 3,
            Precio = 100.00m
        };

        var response = await _client.PostAsJsonAsync("/api/pedidos", dto);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task EliminarPedido_Cliente_DebeRetornar403()
    {
        var token = await ObtenerTokenClienteAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.DeleteAsync("/api/pedidos/1");
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task ActualizarPedido_Cliente_DebeRetornar403()
    {
        var token = await ObtenerTokenClienteAsync();
        _client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var dto = new PedidoRequestDto
        {
            NombreCliente = "Juan Pérez",
            Telefono = "6181234567",
            TipoServicio = "Servicio Militar",
            FechaEntrega = DateTime.Now.AddDays(5),
            CantidadFotos = 5,
            Precio = 150.00m
        };

        var response = await _client.PutAsJsonAsync("/api/pedidos/1", dto);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // ✅ INPUT SANITIZATION - casos adicionales
[Fact]
public async Task Peticion_ConJavascriptEnQuery_DebeRetornar400()
{
    var response = await _client.GetAsync("/api/pedidos?nombre=javascript:alert(1)");
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

[Fact]
public async Task Peticion_ConOnloadEnQuery_DebeRetornar400()
{
    var response = await _client.GetAsync("/api/pedidos?nombre=<img onload=alert(1)>");
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

[Fact]
public async Task Peticion_ConObjectEnQuery_DebeRetornar400()
{
    var response = await _client.GetAsync("/api/pedidos?nombre=<object data='malicious'>");
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

[Fact]
public async Task Peticion_ConEmbedEnQuery_DebeRetornar400()
{
    var response = await _client.GetAsync("/api/pedidos?nombre=<embed src='malicious'>");
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

[Fact]
public async Task Peticion_ConOnerrorEnQuery_DebeRetornar400()
{
    var response = await _client.GetAsync("/api/pedidos?nombre=<img onerror=alert(1)>");
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
}

}