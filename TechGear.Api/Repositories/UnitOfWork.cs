using TechGear.Api.Data;
using TechGear.Api.Entities;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    // Backing fields para Lazy Loading (Mantenemos tu lógica)
    private IProductRepository? _products;
    private IGenericRepository<Brand>? _brands; 
    private ICategoryRepository? _categories; 

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. Productos
    public IProductRepository Products => 
        _products ??= new ProductRepository(_context);

    // 2. Marcas (Aquí estaba el conflicto, ahora usa la implementación lazy)
    public IGenericRepository<Brand> Brands => 
        _brands ??= new GenericRepository<Brand>(_context);

    // 3. Categorías
    public ICategoryRepository Categories => 
        _categories ??= new CategoryRepository(_context);

    // === ELIMINAMOS LA LÍNEA QUE DABA EL ERROR ===
    // IGenericRepository<Brand> IUnitOfWork.Brands => throw new NotImplementedException(); <-- ESTA LINEA SE VA

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this); // Buena práctica agregar esto
    }
}