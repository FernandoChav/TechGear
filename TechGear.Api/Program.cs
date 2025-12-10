using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TechGear.Api.Data;
using TechGear.Api.Entities;
using Mapster;
using TechGear.Api.Interfaces;
using TechGear.Api.DTOs;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar la conexión a Base de Datos (PostgreSQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. Configurar Identity (Seguridad)
// Esto conecta EF Core con el sistema de usuarios
builder.Services.AddScoped<IUnitOfWork, TechGear.Api.Repositories.UnitOfWork>();
builder.Services.AddScoped<IImageService, TechGear.Api.Services.CloudinaryService>();
builder.Services.AddScoped<IProductService, TechGear.Api.Services.ProductService>();
builder.Services.AddIdentity<User, IdentityRole>(options => 
{
    // Configuración laxa para desarrollo (en prod usaríamos reglas estrictas)
    options.Password.RequireDigit = false; 
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Agregar Controladores y Swagger (Documentación)

TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
TypeAdapterConfig<Product, ProductDto>
    .NewConfig()
    .Map(dest => dest.BrandName, src => src.Brand.Name)
    .Map(dest => dest.BrandSlug, src => src.Brand.Slug)
    .Map(dest => dest.CategoryName, src => src.Category.Name)
    .Map(dest => dest.CategorySlug, src => src.Category.Slug)
    .Map(dest => dest.Price, src => src.Variants.Any() ? src.Variants.Min(v => v.Price) : 0) // Precio "desde"
    .Map(dest => dest.PictureUrl, src => src.Images.Any() ? src.Images.First().ImageUrl : ""); // Primera foto
var app = builder.Build();
app.UseMiddleware<TechGear.Api.Middleware.ExceptionMiddleware>();
app.UseHttpsRedirection();  

// Importante: Authentication va antes de Authorization
app.UseAuthentication(); 
app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        // Opcional: Ejecutar migraciones automáticamente al iniciar
        // await context.Database.MigrateAsync();

        // Ejecutar el Seeder
        await DbInitializer.SeedAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al sembrar la base de datos.");
    }
}
app.Run();