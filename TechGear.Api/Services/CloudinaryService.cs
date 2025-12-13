using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Services;

public class CloudinaryService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        // Validación defensiva por si faltan las keys en el appsettings
        var cloudName = config["Cloudinary:CloudName"];
        var apiKey = config["Cloudinary:ApiKey"];
        var apiSecret = config["Cloudinary:ApiSecret"];

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            throw new Exception("Faltan las credenciales de Cloudinary en appsettings.json");
        }

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true; // Forzar HTTPS siempre
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0) return string.Empty;

        // Validar que sea una imagen real (Opcional pero recomendado)
        if (!file.ContentType.StartsWith("image/")) return string.Empty;

        using var stream = file.OpenReadStream();
        
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            
            // === ESTRATEGIA DE ESTANDARIZACIÓN ===
            // 1. Pad: Mantiene la proporción original (no estira ni corta teclados largos).
            // 2. 1000x1000: Resolución alta estándar para zoom.
            // 3. Background White: Rellena los huecos con blanco puro.
            // 4. Auto format/quality: Optimiza el peso del archivo (WebP/AVIF).
            Transformation = new Transformation()
                .Height(1000).Width(1000)
                .Crop("pad") 
                .Background("white")
                .FetchFormat("auto")
                .Quality("auto")
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
        {
            // Podrías loguear el error aquí
            throw new Exception($"Error de Cloudinary: {uploadResult.Error.Message}");
        }

        return uploadResult.SecureUrl?.ToString() ?? string.Empty;
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        var publicId = GetPublicIdFromUrl(imageUrl);
        if (string.IsNullOrEmpty(publicId)) return false;

        var deletionParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deletionParams);

        return result.Result == "ok";
    }

    private string GetPublicIdFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return string.Empty;

        try 
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath; 
            var fileName = Path.GetFileNameWithoutExtension(path); 
            return fileName;
        }
        catch
        {
            return string.Empty;
        }
    }
}