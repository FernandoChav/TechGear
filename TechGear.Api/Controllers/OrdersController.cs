using System.Security.Claims;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechGear.Api.DTOs;
using TechGear.Api.Entities;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Controllers;

[Authorize] // Obligatorio: Solo usuarios logueados pueden ver/crear órdenes
public class OrdersController(IOrderService orderService, IUnitOfWork unitOfWork) : BaseApiController
{
    private readonly IOrderService _orderService = orderService;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    // ==========================================
    // CREAR ORDEN (Desde el Carrito)
    // ==========================================
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Order>>> CreateOrder(OrderDto orderDto)
    {
        // 1. Identificar al usuario
        var email = User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(email)) 
            return Unauthorized(new ApiResponse<string>("No se pudo identificar al usuario"));

        // 2. Convertir DTO de dirección a Entidad
        var address = orderDto.ShipToAddress.Adapt<ShippingAddress>();

        // 3. Llamar al servicio que hace la magia (Redis -> Postgres)
        var order = await _orderService.CreateOrderAsync(email, orderDto.BasketId, address);

        if (order == null) 
            return BadRequest(new ApiResponse<string>("Error al crear la orden. Verifique que el carrito exista y tenga items."));

        return Ok(new ApiResponse<Order>
        {
            Success = true,
            Message = "Orden creada exitosamente",
            Data = order
        });
    }

    // ==========================================
    // HISTORIAL DE ÓRDENES (Lectura)
    // ==========================================
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<Order>>>> GetOrdersForUser()
    {
        var email = User.FindFirstValue(ClaimTypes.Email);

        // Usamos el repositorio específico que creamos con los Includes
        var orders = await _unitOfWork.Orders.GetOrdersByUserAsync(email!);

        return Ok(new ApiResponse<IEnumerable<Order>>
        {
            Success = true,
            Data = orders
        });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Order>>> GetOrderByIdForUser(Guid id)
    {
        var email = User.FindFirstValue(ClaimTypes.Email);

        var order = await _unitOfWork.Orders.GetOrderByIdAsync(id, email!);

        if (order == null) 
            return NotFound(new ApiResponse<string>("Orden no encontrada"));

        return Ok(new ApiResponse<Order>
        {
            Success = true,
            Data = order
        });
    }
}