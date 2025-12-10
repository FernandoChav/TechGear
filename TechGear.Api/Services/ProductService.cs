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

    public async Task<Product?> CreateProductWithImagesAsync(CreateProductDto request, List<IFormFile> images)
    {
        // 1. Reutilizamos el método de crear producto (ya valida marca, categoría y slug)
        // Esto crea el producto en BD y nos devuelve el ID
        var product = await CreateProductAsync(request);
        
        if (product == null) return null; // Si falló la creación (ej: slug repetido)

        // 2. Subir las imágenes una por una
        foreach (var file in images)
        {
            // Reutilizamos el método de subir imagen que acabamos de hacer
            // Nota: Si una falla, el producto queda creado pero sin esa foto. 
            // En un sistema real usaríamos transacciones complejas, pero para MVP está bien.
            if (file.Length > 0)
            {
                await AddImageAsync(product.Id, file);
            }
        }

        // Devolvemos el producto con todo actualizado
        return product;
    }

    public async Task<Product?> UpdateProductAsync(Guid id, PatchProductDto request)
    {
        // 1. Buscar el producto
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null) return null;

        // 2. Actualizar solo si el valor no es nulo (Lógica PATCH)
        
        if (!string.IsNullOrEmpty(request.Name)) 
            product.Name = request.Name;

        if (!string.IsNullOrEmpty(request.Description)) 
            product.Description = request.Description;
            
        if (request.IsActive.HasValue) 
            product.IsActive = request.IsActive.Value;

        // 3. Validar cambios de relaciones (Si nos mandan IDs nuevos)
        if (request.BrandId.HasValue)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(request.BrandId.Value);
            if (brand != null) product.BrandId = request.BrandId.Value;
        }

        if (request.CategoryId.HasValue)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId.Value);
            if (category != null) product.CategoryId = request.CategoryId.Value;
        }

        // 4. Guardar
        // Entity Framework detecta qué propiedades cambiaron y genera el SQL UPDATE solo para ellas
        await _unitOfWork.SaveChangesAsync();

        return product;
    }

    // ... Implementa el resto (AddVariant, AddImage) siguiendo la misma lógica ...
    // Te dejo AddVariant como ejemplo extra:
    public async Task<ProductVariant?> AddVariantAsync(Guid productId, CreateProductVariantDto request)
    {
        // 1. Obtener el producto padre (Jefe)
        var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(productId);

        if (product == null) return null; // El padre no existe

        // 2. Validar que el SKU no exista ya (Regla de negocio crítica)
        // Buscamos en las variantes que ya tiene este producto cargadas en memoria
        if (product.Variants.Any(v => v.Sku == request.Sku))
        {
            throw new Exception($"El SKU '{request.Sku}' ya existe en este producto.");
        }

        // 3. Crear la Variante (Hijo)
        var variant = new ProductVariant
        {
            ProductId = productId,
            Sku = request.Sku,
            Price = request.Price,
            Stock = request.Stock,

            // Mapster o asignación directa del Diccionario de especificaciones
            Specs = request.Specs ?? new Dictionary<string, string>(),
            IsActive = true
        };

        // 4. Agregar al Padre y Guardar
        // Al agregarlo a la lista del padre, EF Core entiende que debe guardarlo en la tabla ProductVariants
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
        // 1. Validar producto
        var product = await _unitOfWork.Products.GetByIdWithImagesAsync(productId);
        if (product == null) return null;

        // 2. Subir usando TU método (UploadImageAsync)
        var imageUrl = await _imageService.UploadImageAsync(file);

        if (string.IsNullOrEmpty(imageUrl))
            throw new Exception("No se pudo subir la imagen a Cloudinary");

        // 3. Guardar en Base de Datos
        var image = new ProductImage
        {
            ImageUrl = imageUrl,
            ProductId = productId
        };

        product.Images.Add(image);
        await _unitOfWork.SaveChangesAsync();

        return image.ImageUrl;
    }

    public async Task<bool> RemoveImageAsync(Guid productId, Guid imageId)
    {
        var product = await _unitOfWork.Products.GetByIdWithImagesAsync(productId);
        if (product == null) return false;

        // Buscamos la imagen en la lista en memoria
        var image = product.Images.FirstOrDefault(x => x.Id == imageId);

        if (image == null) return false;

        // 1. Borrar de Cloudinary usando TU método
        var cloudSuccess = await _imageService.DeleteImageAsync(image.ImageUrl);

        // Opcional: Si falla en la nube, ¿borramos de la BD igual? 
        // Generalmente sí, para no dejar "basura" en la BD, aunque quede huérfana en la nube.

        // 2. Borrar de la BD
        product.Images.Remove(image);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }
}