namespace TechGear.Api.Entities;

public class Order : BaseAuditableEntity
{
    public Order() { } // Constructor vacío para EF Core

    // Constructor útil para crear la orden
    public Order(IReadOnlyList<OrderItem> orderItems, string buyerEmail, ShippingAddress shipToAddress, decimal subtotal, string paymentIntentId)
    {
        BuyerEmail = buyerEmail;
        ShipToAddress = shipToAddress;
        OrderItems = orderItems;
        Subtotal = subtotal;
        PaymentIntentId = paymentIntentId;
    }

    public string BuyerEmail { get; set; } = string.Empty;
    public DateTimeOffset OrderDate { get; set; } = DateTimeOffset.UtcNow;
    
    public ShippingAddress ShipToAddress { get; set; } = null!;
    
    public IReadOnlyList<OrderItem> OrderItems { get; set; } = [];
    
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // ID de la transacción de pago (Stripe/Paypal)
    public string PaymentIntentId { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    // Método helper para obtener el total (Subtotal + Envío si lo hubiera)
    public decimal GetTotal()
    {
        return Subtotal; // + CostoEnvío si lo implementas
    }
}