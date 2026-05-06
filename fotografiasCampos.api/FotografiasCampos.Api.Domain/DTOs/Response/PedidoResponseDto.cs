namespace FotografiasCampos.Api.Domain.DTOs.Response;

public class PedidoResponseDto
{
    public int Id { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string TipoServicio { get; set; } = string.Empty;
    public DateTime FechaEntrega { get; set; }
    public int CantidadFotos { get; set; }
    public decimal Precio { get; set; }
    public string Username { get; set; } = string.Empty;
}