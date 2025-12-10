using TechGear.Api.DTOs;
using TechGear.Api.Entities;
using TechGear.Api.Helpers;

namespace TechGear.Api.Interfaces;

public interface IProductRepository : IGenericRepository<Product>
{
    // Método especial que el genérico no tiene
    Task<Product?> GetBySlugAsync(string slug);
    Task<Product?> GetByIdWithVariantsAsync(Guid id);
    Task<Product?> GetByIdWithImagesAsync(Guid id);
    Task<PagedList<Product>> GetAllAsync(ProductParams productParams);
}