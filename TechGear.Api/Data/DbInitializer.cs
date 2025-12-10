using Microsoft.AspNetCore.Identity;
using TechGear.Api.Entities;

namespace TechGear.Api.Data;

public static class DbInitializer
{
    // AHORA RECIBIMOS TAMBIÉN EL USERMANAGER
    public static async Task SeedAsync(ApplicationDbContext context, RoleManager<IdentityRole> roleManager, UserManager<User> userManager)
    {
        // 1. Crear Roles
        if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));
        if (!await roleManager.RoleExistsAsync("Customer")) await roleManager.CreateAsync(new IdentityRole("Customer"));

        // 2. Crear Usuario ADMIN MAESTRO (Esto es lo nuevo)
        var adminEmail = "admin@techgear.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrador TechGear",
                EmailConfirmed = true
            };
            
            // Contraseña fuerte por defecto
            await userManager.CreateAsync(adminUser, "Admin123$"); 
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }

        // 3. Crear Marcas y Categorías (Lo que ya tenías)
        if (context.Brands.Any()) return;

        var brands = new List<Brand>
        {
            new() { Name = "Logitech", Slug = "logitech" },
            new() { Name = "Keychron", Slug = "keychron" },
            new() { Name = "Razer", Slug = "razer" },
            new() { Name = "Corsair", Slug = "corsair" }
        };
        await context.Brands.AddRangeAsync(brands);
        await context.SaveChangesAsync();

        var perifericos = new Category { Name = "Periféricos", Slug = "perifericos" };
        var componentes = new Category { Name = "Componentes", Slug = "componentes" };
        await context.Categories.AddRangeAsync(perifericos, componentes);
        await context.SaveChangesAsync();

        var teclados = new Category { Name = "Teclados", Slug = "teclados", ParentId = perifericos.Id };
        var mouses = new Category { Name = "Mouses", Slug = "mouses", ParentId = perifericos.Id };
        var audifonos = new Category { Name = "Audífonos", Slug = "audifonos", ParentId = perifericos.Id };
        await context.Categories.AddRangeAsync(teclados, mouses, audifonos);
        await context.SaveChangesAsync();

        var mecanicos = new Category { Name = "Mecánicos", Slug = "mecanicos", ParentId = teclados.Id };
        var membrana = new Category { Name = "Membrana", Slug = "membrana", ParentId = teclados.Id };
        await context.Categories.AddRangeAsync(mecanicos, membrana);
        
        await context.SaveChangesAsync();
    }
}