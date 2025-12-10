using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using TechGear.Api.Data;
using TechGear.Api.DTOs;
using TechGear.Api.Entities;
using TechGear.Api.Helpers;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Repositories;

// Hereda la lógica base del GenericRepository y cumple con IProductRepository
public class ProductRepository(ApplicationDbContext context) : GenericRepository<Product>(context), IProductRepository
{
    public async Task<Product?> GetBySlugAsync(string slug)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.Slug == slug);
    }
    public async Task<Product?> GetByIdWithVariantsAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Variants) // <--- ¡La magia! Hace el JOIN SQL automático
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    public async Task<Product?> GetByIdWithImagesAsync(Guid id)
    {
        return await _dbSet
            .Include(p => p.Images) // <--- CRÍTICO: Traer las imágenes
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    public async Task<PagedList<Product>> GetAllAsync(ProductParams productParams)
    {
        // 1. Query Base (Includes necesarios para ver nombres de marca/cat)
        var query = _dbSet
            .Include(p => p.Brand)
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .Include(p => p.Images)
            .AsQueryable();

        // 2. Filtrado por Texto (Search)
        if (!string.IsNullOrEmpty(productParams.Search))
        {
            var search = productParams.Search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(search) ||
                                     p.Brand.Name.ToLower().Contains(search));
        }

        // 3. Filtrado por Marca (Slug)
        if (!string.IsNullOrEmpty(productParams.Brand))
        {
            query = query.Where(p => p.Brand.Slug == productParams.Brand.ToLower());
        }

        // 4. Filtrado por Categoría (Slug)
        if (!string.IsNullOrEmpty(productParams.Category))
        {
            query = query.Where(p => p.Category.Slug == productParams.Category.ToLower());
        }

        // 5. Ordenamiento (Sorting)
        query = productParams.Sort switch
        {
            "priceAsc" => query.OrderBy(p => p.Variants.Min(v => v.Price)),
            "priceDesc" => query.OrderByDescending(p => p.Variants.Min(v => v.Price)),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderBy(p => p.Name) // Por defecto A-Z
        };

        // 6. Ejecutar Paginación
        return await PagedList<Product>.CreateAsync(query, productParams.PageIndex, productParams.PageSize);
    }
}