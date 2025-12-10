using Microsoft.EntityFrameworkCore;

namespace TechGear.Api.Helpers;

public class PagedList<T> : List<T>
{
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }

    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        
        this.AddRange(items);
    }

    // Método estático para crear la lista de forma asíncrona desde EF Core
    public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync(); // Cuenta total antes de recortar
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(); // Recorte (Paginación)
        
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}