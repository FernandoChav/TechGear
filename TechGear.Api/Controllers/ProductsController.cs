using System.Text.Json;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using TechGear.Api.DTOs;
using TechGear.Api.Interfaces;
using Microsoft.AspNetCore.Authorization;
namespace TechGear.Api.Controllers;

public class ProductsController(IUnitOfWork unitOfWork, IProductService productService) : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IProductService _productService = productService;

    // ==========================================
    // LECTURA (Queries) -> Directo al Repo (Rápido)
    // ==========================================

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAll([FromQuery] ProductParams productParams)
    {
        var products = await _unitOfWork.Products.GetAllAsync(productParams);

        Response.Headers.Append("X-Pagination", JsonSerializer.Serialize(new 
        {
            products.TotalCount,
            products.PageSize,
            products.CurrentPage,
            products.TotalPages
        }));

        // Adaptamos la Entidad a DTO antes de enviarla
        return OkResponse(products.Adapt<IEnumerable<ProductDto>>(), "Productos listados correctamente");
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetOne(Guid id)
    {
        var product = await _unitOfWork.Products.GetByIdWithVariantsAsync(id);

        // Usamos el Helper HandleResult para ahorrar el if/else
        return HandleResult(product?.Adapt<ProductDto>(), "Producto encontrado");
    }

    // ==========================================
    // ESCRITURA (Commands) -> A través del Service (Seguro)
    // ==========================================
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductDto>>> Create(CreateProductDto request)
    {
        // Delegamos al servicio. Si devuelve null, es porque el slug existe.
        var product = await _productService.CreateProductAsync(request);

        if (product == null)
            return BadRequestResponse<ProductDto>($"El slug '{request.Slug}' ya existe.");

        return CreatedResponse(nameof(GetOne), new { id = product.Id }, product.Adapt<ProductDto>(), "Producto creado exitosamente");
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("with-images")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateWithImages(
        [FromForm] CreateProductDto request,
        [FromForm] List<IFormFile> images)
    {
        // El controlador NO toca Cloudinary. Solo pasa los archivos al servicio.
        var product = await _productService.CreateProductWithImagesAsync(request, images);

        if (product == null)
            return BadRequestResponse<ProductDto>($"Error al crear producto. Verifique que el slug '{request.Slug}' no exista.");

        return CreatedResponse(nameof(GetOne), new { id = product.Id }, product.Adapt<ProductDto>(), "Producto con imágenes creado exitosamente");
    }
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id}")]
    public async Task<ActionResult<ApiResponse<ProductDto>>> PatchUpdate(Guid id, PatchProductDto request)
    {
        var product = await _productService.UpdateProductAsync(id, request);

        if (product == null) 
            return NotFoundResponse<ProductDto>("Producto no encontrado");

        return OkResponse(product.Adapt<ProductDto>(), "Producto actualizado correctamente");
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/variants")]
    public async Task<ActionResult<ApiResponse<ProductVariantDto>>> AddVariant(Guid id, CreateProductVariantDto request)
    {
        var variant = await _productService.AddVariantAsync(id, request);

        if (variant == null)
            return NotFoundResponse<ProductVariantDto>($"El producto padre con ID {id} no existe.");

        return OkResponse(variant.Adapt<ProductVariantDto>(), "Variante agregada al inventario");
    }
    [Authorize(Roles = "Admin")]
    [HttpPost("{id}/images")]
    public async Task<ActionResult<ApiResponse<object>>> UploadImage(Guid id, IFormFile file)
    {
        // Nota: Ya no inyectamos IImageService aquí en el método. El ProductService lo maneja internamente.
        var imageUrl = await _productService.AddImageAsync(id, file);

        if (imageUrl == null)
            return BadRequestResponse<object>("No se pudo subir la imagen (Verifique que el producto exista o el archivo sea válido)");

        return OkResponse<object>(new { url = imageUrl }, "Imagen subida exitosamente");
    }
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}/images/{imageId}")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteImage(Guid id, Guid imageId)
    {
        var success = await _productService.RemoveImageAsync(id, imageId);

        if (!success)
            return BadRequestResponse<object>("No se pudo eliminar la imagen (Puede que no exista o falló la nube)");

        return OkMessage("Imagen eliminada correctamente");
    }
}