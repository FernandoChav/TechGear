using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGear.Api.Entities;
using TechGear.Api.Interfaces;
using TechGear.Api.DTOs;

namespace TechGear.Api.Controllers;

[Authorize]
public class PaymentsController(IUnitOfWork unitOfWork) : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    // POST: api/payments/{orderId}
    // Simula que el usuario pagó exitosamente
    [HttpPost("{orderId}")]
    public async Task<ActionResult<ApiResponse<Order>>> SimulatePayment(Guid orderId)
    {
        // 1. Buscamos la orden
        // Usamos el repositorio genérico para buscar por ID directo
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

        if (order == null) 
            return NotFound(new ApiResponse<string>("Orden no encontrada"));

        // 2. Validamos que no esté pagada ya
        if (order.Status == OrderStatus.PaymentReceived)
            return BadRequest(new ApiResponse<string>("Esta orden ya fue pagada anteriormente"));

        // 3. SIMULACIÓN DEL PAGO
        // Aquí es donde Stripe haría su magia. Nosotros solo cambiamos el estado.
        order.Status = OrderStatus.PaymentReceived;
        
        // Generamos un ID falso de transacción para que parezca real en la BD
        order.PaymentIntentId = $"FAKE_PAY_{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";

        // 4. Guardamos los cambios
        _unitOfWork.Orders.Update(order);
        var result = await _unitOfWork.SaveChangesAsync();

        if (result <= 0) 
            return BadRequest(new ApiResponse<string>("Error al procesar el pago simulado"));

        return Ok(new ApiResponse<Order>
        {
            Success = true,
            Message = "Pago realizado exitosamente (Simulado)",
            Data = order
        });
    }
}