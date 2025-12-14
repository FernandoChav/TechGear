using TechGear.Api.DTOs;
using TechGear.Api.Entities;

namespace TechGear.Api.Interfaces;

public interface IOrderService
{
    Task<Order?> CreateOrderAsync(string buyerEmail, string basketId, ShippingAddress shippingAddress);
    // Más adelante aquí irán métodos como: GetOrdersByUserAsync, GetOrderByIdAsync...
}