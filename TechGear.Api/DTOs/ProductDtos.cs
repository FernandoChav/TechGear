using System.ComponentModel.DataAnnotations;

namespace TechGear.Api.DTOs;

// Lo que recibimos para crear un producto
public record CreateProductDto(
    [Required] string Name, 
    [Required] string Brand,
    [Required] string Category,
    [Required] string Description,
    [Required] string Slug
);

// Lo que devolvemos al cliente (sin datos sensibles si los hubiera)
public record ProductDto(
    Guid Id, 
    string Name, 
    string Slug, 
    string Description,
    decimal Price, // Precio calculado (el más bajo de las variantes)
    string PictureUrl, // La imagen principal
    
    // INFO APLANADA (Flat Data)
    string BrandName,     // En lugar de BrandId
    string BrandSlug,
    string CategoryName,  // En lugar de CategoryId
    string CategorySlug,

    // Colecciones (Opcional, a veces en listas grandes no queremos variantes)
    // IEnumerable<ProductVariantDto> Variants 
    bool IsActive,
    IEnumerable<ProductImageDto> Images
);
// Lo que recibimos para crear una variante (El inventario físico)
public record CreateProductVariantDto(
    [Required] string Sku,
    [Required] decimal Price,
    [Required] int Stock,
    // Aquí recibimos el JSON flexible (ej: { "Color": "Negro", "Switch": "Brown" })
    Dictionary<string, string> Specs 
);

// Lo que devolvemos al cliente (La variante con su ID)
public record ProductVariantDto(
    Guid Id,
    string Sku,
    decimal Price,
    int Stock,
    Dictionary<string, string> Specs
);

public record PatchProductDto(
    string? Name,        
    string? Description, 
    bool? IsActive,      
    Guid? BrandId,       
    Guid? CategoryId     
);
public record ProductImageDto(
    Guid Id,
    string ImageUrl
);