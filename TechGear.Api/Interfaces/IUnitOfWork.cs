using TechGear.Api.Entities;
using TechGear.Api.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; } // <--- Agrega esto
    Task<int> SaveChangesAsync();
    IGenericRepository<Brand> Brands { get; } // Para marcas usamos el genérico, es suficiente
    ICategoryRepository Categories { get; }
    IOrderRepository Orders { get; }
}