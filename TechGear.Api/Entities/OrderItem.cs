namespace TechGear.Api.Entities;

public class OrderItem : BaseAuditableEntity
{
    // Foto del producto (Snapshot)
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string PictureUrl { get; set; } = string.Empty;

    public decimal Price { get; set; } // Precio al momento de la compra
    public int Quantity { get; set; }
}