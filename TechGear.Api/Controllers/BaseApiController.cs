using Microsoft.AspNetCore.Mvc;
using TechGear.Api.DTOs;

namespace TechGear.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    // --- RESPUESTAS DE ÉXITO ---

    // 1. Éxito con Datos (200 OK)
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data, string message = "Operación exitosa")
    {
        return Ok(new ApiResponse<T>(data, message));
    }

    // 2. Éxito solo Mensaje (200 OK - Ideal para Delete/Update con Envelope)
    // Usamos <object> porque no hay datos reales que devolver
    protected ActionResult<ApiResponse<object>> OkMessage(string message = "Operación exitosa")
    {
        return Ok(new ApiResponse<object>(message));
    }

    // 3. Recurso Creado (201 Created)
    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(string actionName, object routeValues, T data, string message = "Recurso creado")
    {
        return CreatedAtAction(actionName, routeValues, new ApiResponse<T>(data, message));
    }

    // 4. No Content Real (204)
    // OJO: Esto devolverá vacío TOTAL, sin JSON. Usar solo si el cliente lo prefiere así.
    protected ActionResult NoContentResponse()
    {
        return NoContent();
    }

    // --- RESPUESTAS DE ERROR ---

    // 5. Error de Cliente (400 Bad Request)
    protected ActionResult<ApiResponse<T>> BadRequestResponse<T>(string message, IEnumerable<string>? errors = null)
    {
        return BadRequest(new ApiResponse<T>(message, errors));
    }

    // 6. No Encontrado (404 Not Found)
    protected ActionResult<ApiResponse<T>> NotFoundResponse<T>(string message = "Recurso no encontrado")
    {
        return NotFound(new ApiResponse<T>(message));
    }

    // --- UTILIDAD "HANDLE RESULT" (Nivel Pro) ---
    
    // Este método toma el resultado de una operación (null o data) y decide qué devolver.
    // Ahorra muchísimos "if/else" en tus controladores.
    protected ActionResult<ApiResponse<T>> HandleResult<T>(T? result, string successMessage = "Ok", string notFoundMessage = "No encontrado")
    {
        if (result == null)
            return NotFoundResponse<T>(notFoundMessage);

        return OkResponse(result, successMessage);
    }
}