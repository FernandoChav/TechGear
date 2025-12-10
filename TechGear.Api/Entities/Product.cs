using System.ComponentModel.DataAnnotations;

namespace TechGear.Api.Entities;

public class Product : BaseAuditableEntity
{
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // El Slug es vital para el SEO (ej: /producto/teclado-k2) en lugar de usar IDs feos en la URL
    [MaxLength(150)]
    public string Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Guid BrandId { get; set; }
    public Brand Brand { get; set; } = null!;

    // Relación con Categoría
    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    // Relación: Un producto tiene muchas variantes
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();

    
    // AGREGA ESTO:
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
}