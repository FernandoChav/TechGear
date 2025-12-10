using TechGear.Api.Data;
using TechGear.Api.Entities;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    private IProductRepository? _products;
    private IGenericRepository<Brand>? _brands; // <--- Nuevo
    private ICategoryRepository? _categories;   // <--- Nuevo

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IProductRepository Products =>
        _products ??= new ProductRepository(_context);

    // Repositorio Genérico "al vuelo" para Marcas
    public IGenericRepository<Brand> Brands =>
        _brands ??= new GenericRepository<Brand>(_context);

    // Repositorio Especializado para Categorías
    public ICategoryRepository Categories =>
        _categories ??= new CategoryRepository(_context);

    IGenericRepository<Brand> IUnitOfWork.Brands => throw new NotImplementedException();

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}