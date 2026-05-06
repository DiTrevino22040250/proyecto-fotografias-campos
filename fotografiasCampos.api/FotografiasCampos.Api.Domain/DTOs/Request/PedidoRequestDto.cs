 using System.ComponentModel.DataAnnotations;

namespace FotografiasCampos.Api.Domain.DTOs.Request;

public class PedidoRequestDto
{
    [Required]
    [StringLength(100)]
    public string NombreCliente { get; set; } = string.Empty;

    [Required]
    [Phone]
    public string Telefono { get; set; } = string.Empty;

    [Required]
    public string TipoServicio { get; set; } = string.Empty;

    [Required]
    public DateTime FechaEntrega { get; set; }

    [Range(1, 30)]
    public int CantidadFotos { get; set; }

    [Range(0.01, 99999.99)]
    public decimal Precio { get; set; }
}
