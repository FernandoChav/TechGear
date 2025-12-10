using TechGear.Api.Entities;

namespace TechGear.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // 1. Asegurarse de que la BD existe (opcional si usas migraciones, pero útil)
        // await context.Database.EnsureCreatedAsync(); 

        // 2. Verificar si ya existen Marcas. Si hay, no hacemos nada.
        if (context.Brands.Any())
        {
            return; // La DB ya fue sembrada
        }

        // 3. Crear Marcas
        var brands = new List<Brand>
        {
            new() { Name = "Logitech", Slug = "logitech" },
            new() { Name = "Keychron", Slug = "keychron" },
            new() { Name = "Razer", Slug = "razer" },
            new() { Name = "Corsair", Slug = "corsair" }
        };
        await context.Brands.AddRangeAsync(brands);
        await context.SaveChangesAsync(); // Guardamos para obtener los IDs

        // 4. Crear Categorías y Subcategorías
        // Recuperamos las entidades adjuntas para que EF no intente crearlas de nuevo
        
        var perifericos = new Category { Name = "Periféricos", Slug = "perifericos" };
        var componentes = new Category { Name = "Componentes", Slug = "componentes" };

        await context.Categories.AddRangeAsync(perifericos, componentes);
        await context.SaveChangesAsync();

        // Subcategorías (usamos el objeto padre 'perifericos')
        var teclados = new Category { Name = "Teclados", Slug = "teclados", ParentId = perifericos.Id };
        var mouses = new Category { Name = "Mouses", Slug = "mouses", ParentId = perifericos.Id };
        var audifonos = new Category { Name = "Audífonos", Slug = "audifonos", ParentId = perifericos.Id };

        await context.Categories.AddRangeAsync(teclados, mouses, audifonos);
        await context.SaveChangesAsync();

        var mecanicos = new Category { Name = "Mecánicos", Slug = "mecanicos", ParentId = teclados.Id };
        var membrana = new Category { Name = "Membrana", Slug = "membrana", ParentId = teclados.Id };

        await context.Categories.AddRangeAsync(mecanicos, membrana);
        
        // 5. Guardar todo el grafo final
        await context.SaveChangesAsync();
    }
}