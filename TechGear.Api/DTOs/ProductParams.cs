namespace TechGear.Api.DTOs;

public class ProductParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    // Filtros
    public string? Sort { get; set; }      // ej: "priceAsc", "priceDesc"
    public string? Search { get; set; }    // ej: "teclado gamer"
    public string? Brand { get; set; }     // ej: "logitech" (usaremos el Slug)
    public string? Category { get; set; }  // ej: "teclados" (usaremos el Slug)

    // Paginación
    public int PageIndex { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}