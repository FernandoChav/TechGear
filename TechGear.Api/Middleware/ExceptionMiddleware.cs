using System.Net;
using System.Text.Json;
using TechGear.Api.DTOs;

namespace TechGear.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;
    private readonly IHostEnvironment _env = env;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Intenta ejecutar el request normal
            await _next(context);
        }
        catch (Exception ex)
        {
            // Si algo explota, caemos aquí
            _logger.LogError(ex, ex.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            // En Desarrollo mostramos el error real (StackTrace). En Producción solo "Internal Server Error".
            var message = _env.IsDevelopment() ? ex.Message : "Error interno del servidor";
            var stackTrace = _env.IsDevelopment() ? ex.StackTrace?.ToString() : null;

            var response = new ApiResponse<string>(message, new[] { stackTrace ?? "Ver logs para más detalles" })
            {
                Success = false
            };

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}