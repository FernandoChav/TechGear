using Microsoft.AspNetCore.Mvc;
using TechGear.Api.DTOs;
using TechGear.Api.Entities;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Controllers;

public class BasketController(IBasketRepository basketRepository) : BaseApiController
{
    private readonly IBasketRepository _basketRepository = basketRepository;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<CustomerBasket>>> GetBasketById(string id)
    {
        var basket = await _basketRepository.GetBasketAsync(id);

        // Si no existe, retornamos uno nuevo vacío para no romper el front
        return Ok(new ApiResponse<CustomerBasket>
        {
            Success = true,
            Data = basket ?? new CustomerBasket(id)
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerBasket>>> UpdateBasket(CustomerBasket basket)
    {
        var updatedBasket = await _basketRepository.UpdateBasketAsync(basket);

        return Ok(new ApiResponse<CustomerBasket>
        {
            Success = true,
            Message = "Carrito actualizado",
            Data = updatedBasket
        });
    }

    [HttpDelete]
    public async Task<ActionResult<ApiResponse<object>>> DeleteBasket(string id)
    {
        await _basketRepository.DeleteBasketAsync(id);
        return Ok(new ApiResponse<object>("Carrito eliminado"));
    }
}