using Mapster;
using Microsoft.AspNetCore.Mvc;
using TechGear.Api.DTOs;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Controllers;


public class BrandsController(IUnitOfWork unitOfWork) : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<BrandDto>>>> GetAll()
    {
        var brands = await _unitOfWork.Brands.GetAllAsync();
        return OkResponse(brands.Adapt<IEnumerable<BrandDto>>(), "Marcas listadas correctamente");
    }
}