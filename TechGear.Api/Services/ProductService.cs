using Mapster;
using TechGear.Api.DTOs;
using TechGear.Api.Entities;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageService _imageService;

    // Inyectamos UnitOfWork y ImageService AQUÍ, no en el controlador
    public ProductService(IUnitOfWork unitOfWork, IImageService imageService)
    {
        _unitOfWork = unitOfWork;
        _imageService = imageService;
    }

    public async Task<Product?> CreateProductWithImagesAsync(CreateProductDto dto, List<IFormFile> images)
    {
        // 1. Validar Slug
        var existing = await _unitOfWork.Products.GetBySlugAsync(dto.Slug);
        if (existing != null) return null; // O lanzar excepción personalizada

        // 2. Mapear
        var product = dto.Adapt<Product>();

        // 3. Subir Imágenes
        if (images != null && images.Any())
        {
            product.Images = new List<ProductImage>();
            foreach (var file in images)
            {
                var url = await _imageService.UploadImageAsync(file);
                if (!string.IsNullOrEmpty(url))
                {
                    product.Images.Add(new ProductImage { ImageUrl = url });
                }
            }
        }

        // 4. Guardar
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        return product;
    }

    public async Task<Product?> UpdateProductAsync(Guid id, PatchProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return null;

        // Lógica de parcheo
        if (dto.Name != null) product.Name = dto.Name;
        if (dto.Description != null) product.Description = dto.Description;
        if (dto.IsActive.HasValue) product.IsActive = dto.IsActive.Value;
        if (dto.BrandId.HasValue) product.BrandId = dto.BrandId.Value;
        if (dto.CategoryId.HasValue) product.CategoryId = dto.CategoryId.Value;

        await _unitOfWork.SaveChangesAsync();
        return product;
    }

    // ... Implementa el resto (AddVariant, AddImage) siguiendo la misma lógica ...
    // Te dejo AddVariant como ejemplo extra:
    public async Task<ProductVariant?> AddVariantAsync(Guid productId, CreateProductVariantDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null) return null;

        var variant = dto.Adapt<ProductVariant>();
        variant.ProductId = productId;

        if (product.Variants == null) product.Variants = new List<ProductVariant>();
        product.Variants.Add(variant);

        await _unitOfWork.SaveChangesAsync();
        return variant;
    }

    public async Task<Product?> CreateProductAsync(CreateProductDto dto)
    {
        var existing = await _unitOfWork.Products.GetBySlugAsync(dto.Slug);
        if (existing != null) return null; // O lanzar excepción personalizada
        var product = dto.Adapt<Product>();
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();
        return product;
    }

    public async Task<string?> AddImageAsync(Guid productId, IFormFile file)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null) return null;
        var url = await _imageService.UploadImageAsync(file);
        if (string.IsNullOrEmpty(url)) return null;
        if (product.Images == null) product.Images = new List<ProductImage>();
        var image = new ProductImage { ImageUrl = url };
        product.Images.Add(image);
        await _unitOfWork.SaveChangesAsync();
        return url;
    }

    public async Task<bool> RemoveImageAsync(Guid productId, Guid imageId)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null) return false;

        var images = product.Images;
        if (images == null) return false;

        var image = images.FirstOrDefault(img => img.Id == imageId);
        if (image == null) return false;

        images.Remove(image);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}