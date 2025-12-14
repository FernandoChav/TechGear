using TechGear.Api.Entities;

namespace TechGear.Api.Interfaces;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByUserAsync(string buyerEmail);
    Task<Order?> GetOrderByIdAsync(Guid id, string buyerEmail);
}