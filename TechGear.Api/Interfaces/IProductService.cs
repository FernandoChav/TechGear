using TechGear.Api.DTOs;
using TechGear.Api.Entities;

namespace TechGear.Api.Interfaces;

public interface IProductService
{
    // Solo definimos las operaciones "pesadas" o de escritura
    Task<Product?> CreateProductAsync(CreateProductDto dto);
    Task<Product?> CreateProductWithImagesAsync(CreateProductDto dto, List<IFormFile> images);
    Task<Product?> UpdateProductAsync(Guid id, PatchProductDto dto);
    Task<ProductVariant?> AddVariantAsync(Guid productId, CreateProductVariantDto dto);
    Task<string?> AddImageAsync(Guid productId, IFormFile file);
    Task<bool> RemoveImageAsync(Guid productId, Guid imageId);
}