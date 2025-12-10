using Mapster;
using Microsoft.AspNetCore.Mvc;
using TechGear.Api.DTOs;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Controllers;


public class CategoriesController(IUnitOfWork unitOfWork) : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CategoryDto>>>> GetAll()
    {
        // Usamos el método especial que trae la jerarquía
        var categories = await _unitOfWork.Categories.GetAllWithTreeAsync();
        
        // Mapster es inteligente: mapeará recursivamente Children -> SubCategories
        return OkResponse(categories.Adapt<IEnumerable<CategoryDto>>(), "Categorías listadas correctamente");
    }
}