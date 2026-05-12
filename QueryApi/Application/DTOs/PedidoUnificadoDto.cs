namespace QueryApi.Application.DTOs;

public class PedidoUnificadoDto
{
    public int Id { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string TipoServicio { get; set; } = string.Empty;
    public DateTime FechaEntrega { get; set; }
    public int CantidadFotos { get; set; }
    public decimal Precio { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
}


