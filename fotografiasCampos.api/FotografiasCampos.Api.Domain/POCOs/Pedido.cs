namespace FotografiasCampos.Api.Domain.POCOs;

public class Pedido
{
    public int Id { get; set; }
    public string NombreCliente { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string TipoServicio { get; set; } = string.Empty;
    public DateTime FechaEntrega { get; set; }
    public int CantidadFotos { get; set; }
    public decimal Precio { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}