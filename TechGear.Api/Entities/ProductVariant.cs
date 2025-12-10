using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechGear.Api.Entities;

public class ProductVariant : BaseAuditableEntity
{
    // Clave foránea al padre
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    [MaxLength(50)]
    public string Sku { get; set; } = string.Empty; // Código único de inventario

    [Column(TypeName = "decimal(18,2)")] // Importante para manejar dinero sin errores de redondeo
    public decimal Price { get; set; }

    public int Stock { get; set; }

    // PostgreSQL JSONB: Guardamos atributos dinámicos aquí
    public Dictionary<string, string> Specs { get; set; } = new();
}