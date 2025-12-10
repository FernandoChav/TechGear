using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using TechGear.Api.Data;
using TechGear.Api.Entities;
using Mapster;
using TechGear.Api.Interfaces;
using TechGear.Api.DTOs;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

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
builder.Services.AddScoped<ITokenService, TechGear.Api.Services.TokenService>();
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
var tokenKey = builder.Configuration["JwtSettings:TokenKey"]
    ?? throw new Exception("No se encontró la llave del token en appsettings");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
        ValidateIssuer = false, // En desarrollo simplificamos esto
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Importante para que expire exactamente cuando decimos
    };

    // Configuración vital para leer el token desde la Cookie (y no del Header Authorization)
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Buscamos la cookie llamada "accessToken"
            var accessToken = context.Request.Cookies["accessToken"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});
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

        // AGREGAR ESTA LÍNEA:
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // PASAR LOS DOS PARÁMETROS:
        await DbInitializer.SeedAsync(context, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al sembrar la base de datos.");
    }
}
app.Run();