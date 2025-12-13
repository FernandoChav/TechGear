namespace TechGear.Api.Entities;

public class BasketItem
{
    public Guid Id { get; set; } // ID del Producto original
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string PictureUrl { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}