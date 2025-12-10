namespace TechGear.Api.DTOs;

// Para las Marcas: Simple y plano
public record BrandDto(Guid Id, string Name, string Slug);

// Para las Categorías: Jerárquico (Árbol)
public record CategoryDto(
    Guid Id, 
    string Name, 
    string Slug, 
    // Lista recursiva: Una categoría contiene una lista de sí misma (Hijos)
    IEnumerable<CategoryDto> SubCategories 
);