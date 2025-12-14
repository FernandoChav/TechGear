using Microsoft.EntityFrameworkCore;
using TechGear.Api.Data;
using TechGear.Api.Entities;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Repositories;

public class OrderRepository(ApplicationDbContext context) : GenericRepository<Order>(context), IOrderRepository
{


    public async Task<IEnumerable<Order>> GetOrdersByUserAsync(string buyerEmail)
    {
        // Usamos _context (heredado del Genérico)
        return await _context.Orders
            .Where(o => o.BuyerEmail == buyerEmail)
            .Include(o => o.OrderItems) 
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<Order?> GetOrderByIdAsync(Guid id, string buyerEmail)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id && o.BuyerEmail == buyerEmail);
    }
}