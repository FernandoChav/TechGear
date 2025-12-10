using Microsoft.EntityFrameworkCore;
using TechGear.Api.Data;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(T entity)
    {
        if (entity is Entities.BaseAuditableEntity auditable)
        {
            // Soft Delete: Si es una entidad nuestra, solo la marcamos como borrada
            auditable.IsDeleted = true;
            Update(entity);
        }
        else
        {
            // Hard Delete: Si es otra cosa, la borramos de verdad
            _dbSet.Remove(entity);
        }
    }
}