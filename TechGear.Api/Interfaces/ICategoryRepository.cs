using TechGear.Api.Entities;

namespace TechGear.Api.Interfaces;

public interface ICategoryRepository : IGenericRepository<Category>
{
    // Solo queremos las categorías raíz (Padres), con sus hijos cargados
    Task<IEnumerable<Category>> GetAllWithTreeAsync();
}