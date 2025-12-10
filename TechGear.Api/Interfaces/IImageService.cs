using Microsoft.AspNetCore.Http;

namespace TechGear.Api.Interfaces;

public interface IImageService
{
    // Recibe el archivo crudo del formulario y devuelve la URL segura (https)
    Task<string> UploadImageAsync(IFormFile file);
    
    Task<bool> DeleteImageAsync(string imageUrl);
}