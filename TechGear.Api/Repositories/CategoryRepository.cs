using Microsoft.EntityFrameworkCore;
using TechGear.Api.Data;
using TechGear.Api.Entities;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Category>> GetAllWithTreeAsync()
    {
        return await _dbSet
            .Include(c => c.SubCategories) // Carga el primer nivel de hijos
            .Where(c => c.ParentId == null) // Filtro: Solo traigo los padres (Raíz)
            .ToListAsync();
    }
}