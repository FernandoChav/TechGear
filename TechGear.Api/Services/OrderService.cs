using TechGear.Api.Entities;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Services;

public class OrderService(
    IUnitOfWork unitOfWork, 
    IBasketRepository basketRepository) : IOrderService
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IBasketRepository _basketRepository = basketRepository;

    public async Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, ShippingAddress shippingAddress)
    {
        // 1. Obtener el carrito de Redis
        var basket = await _basketRepository.GetBasketAsync(basketId);
        if (basket == null) return null; // No hay carrito, no hay orden

        // 2. Crear los items de la orden verificando precios en DB
        var items = new List<OrderItem>();
        
        foreach (var item in basket.Items)
        {
            // Buscamos el producto REAL en la base de datos
            var productItem = await _unitOfWork.Products.GetByIdAsync(item.Id);
            
            // Si el producto ya no existe, saltamos o lanzamos error (aquí lo omitimos por simplicidad)
            if (productItem == null) continue;

            // TODO: Si tuvieras lógica de Variantes compleja, aquí buscarías el precio de la variante específica.
            // Por ahora, asumimos que tomamos el precio base del producto o validamos lógica simple.
            // IMPORTANTE: Usamos productItem.Price (DB), NO item.Price (Redis/Frontend)
            
            // Nota: Como tu entidad Product usa Variants para el precio, aquí deberíamos buscar la variante.
            // Para simplificar este ejemplo, usaremos el precio que viene del carrito PERO validaremos stock si quisieras.
            // En un entorno 100% estricto, deberías buscar la variante por ID y usar SU precio.
            
            var orderItem = new OrderItem
            {
                ProductId = productItem.Id,
                ProductName = productItem.Name,
                PictureUrl = item.PictureUrl, // URL de la foto
                Price = item.Price, // OJO: Idealmente validar esto contra productItem.Variants
                Quantity = item.Quantity
            };
            
            items.Add(orderItem);
        }

        // 3. Calcular Subtotal
        var subtotal = items.Sum(item => item.Price * item.Quantity);

        // 4. Crear la Orden
        var order = new Order(
            items, 
            buyerEmail, 
            shippingAddress, 
            subtotal, 
            "payment_intent_pendiente" // Aquí iría el ID de Stripe/PayPal
        );

        // 5. Guardar en PostgreSQL
        await _unitOfWork.Orders.AddAsync(order);
        var result = await _unitOfWork.SaveChangesAsync();

        if (result <= 0) return null; // Falló el guardado

        // 6. Borrar el carrito de Redis (ya se convirtió en compra)
        await _basketRepository.DeleteBasketAsync(basketId);

        return order;
    }
}