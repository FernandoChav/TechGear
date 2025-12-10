using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Services;

public class CloudinaryService : IImageService
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryService(IConfiguration config)
    {
        // Leemos la configuración del appsettings.json
        var account = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]
        );

        _cloudinary = new Cloudinary(account);
    }

    public async Task<string> UploadImageAsync(IFormFile file)
    {
        if (file.Length == 0) return string.Empty;

        // Preparamos el stream de datos
        using var stream = file.OpenReadStream();
        
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            // Opcional: Transformación automática (recortar a cuadrado 500x500)
            Transformation = new Transformation().Height(500).Width(500).Crop("fill")
        };

        // Subimos a la nube
        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        // Devolvemos la URL pública
        return uploadResult.SecureUrl?.ToString() ?? string.Empty;
    }
    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        // 1. Extraer el PublicId de la URL
        var publicId = GetPublicIdFromUrl(imageUrl);
        if (string.IsNullOrEmpty(publicId)) return false;

        // 2. Ejecutar comando de borrado en Cloudinary
        var deletionParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deletionParams);

        // 3. Verificar si Cloudinary dijo "ok"
        return result.Result == "ok";
    }

    // Método privado auxiliar para parsear la URL
    private string GetPublicIdFromUrl(string url)
    {
        // Las URLs de Cloudinary suelen ser: .../upload/v12345/NOMBRE_ARCHIVO.jpg
        // Necesitamos solo "NOMBRE_ARCHIVO" (sin la extensión)
        
        try 
        {
            var uri = new Uri(url);
            var path = uri.AbsolutePath; // /dn123/image/upload/v123/foto.jpg
            var fileName = Path.GetFileNameWithoutExtension(path); 
            // NOTA: Si usas carpetas en Cloudinary, la lógica es más compleja, 
            // pero para subidas directas esto funciona el 99% de las veces.
            return fileName;
        }
        catch
        {
            return string.Empty;
        }
    }
}