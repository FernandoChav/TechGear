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

    public async Task<Product?> CreateProductAsync(CreateProductDto request)
    {
        // 1. Validar que el Slug sea único
        var existingProduct = (await _unitOfWork.Products.GetAllAsync())
                              .FirstOrDefault(p => p.Slug == request.Slug);
        if (existingProduct != null) return null;

        // 2. BUSCAR LA MARCA POR NOMBRE (Aquí arreglamos el error del Cast)
        // Traemos todas las marcas y buscamos la que coincida con el texto que enviaste
        var allBrands = await _unitOfWork.Brands.GetAllAsync();
        var brand = allBrands.FirstOrDefault(b => b.Name.ToLower() == request.Brand.ToLower());

        if (brand == null)
        {
            // Opcional: Podríamos lanzar una excepción o crear la marca al vuelo.
            // Por ahora, retornamos null para indicar error.
            throw new Exception($"La marca '{request.Brand}' no existe en la base de datos.");
        }

        // 3. BUSCAR LA CATEGORÍA POR NOMBRE
        var allCategories = await _unitOfWork.Categories.GetAllAsync();
        var category = allCategories.FirstOrDefault(c => c.Name.ToLower() == request.Category.ToLower());

        if (category == null)
        {
            throw new Exception($"La categoría '{request.Category}' no existe en la base de datos.");
        }

        // 4. CREAR LA ENTIDAD MANUALMENTE (Más seguro que Mapster en este caso)
        // Así evitamos que intente meter el string en el objeto
        var product = new Product
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            IsActive = true,

            // Aquí asignamos los IDs que encontramos arriba
            BrandId = brand.Id,
            CategoryId = category.Id
        };

        // 5. Guardar en Base de Datos
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