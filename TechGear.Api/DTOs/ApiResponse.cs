namespace TechGear.Api.DTOs;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public ApiResponse() { }

    // 1. ÉXITO CON DATOS (GET, POST con retorno)
    public ApiResponse(T data, string message = "Success")
    {
        Success = true;
        Message = message;
        Data = data;
        Errors = null;
    }

    // 2. ÉXITO SIN DATOS (NUEVO) - Para operaciones como Delete/Update
    // Usamos un constructor que solo recibe mensaje y asume éxito
    public ApiResponse(string message)
    {
        Success = true;
        Message = message;
        Data = default;
        Errors = null;
    }

    // 3. ERROR (400, 404, 500)
    public ApiResponse(string message, IEnumerable<string>? errors = null)
    {
        Success = false;
        Message = message;
        Data = default;
        Errors = errors;
    }
}