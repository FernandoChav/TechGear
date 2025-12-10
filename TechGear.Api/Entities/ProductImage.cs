using System.ComponentModel.DataAnnotations;

namespace TechGear.Api.Entities;

public class ProductImage : BaseAuditableEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    // Aquí guardaremos la URL que nos devuelva la nube (ej: https://res.cloudinary.com/...)
    [Required]
    public string ImageUrl { get; set; } = string.Empty;
}