using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechGear.Api.Entities;

namespace TechGear.Api.Data;

// Solo debe haber UNA herencia después de los dos puntos (: IdentityDbContext<User>)
public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); 

        
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        builder.HasDefaultSchema("public");

        

        // 1. Configuración de Product
        builder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Slug).IsUnique(); // No puede haber 2 URLs iguales
            entity.HasIndex(p => p.Name); // Índice para búsqueda rápida por nombre
        });

        // 2. Configuración de ProductVariant
        builder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(v => v.Sku).IsUnique(); // No puede haber 2 SKU iguales

            // Relación: Si borro un Producto, se borran sus variantes (Cascade)
            entity.HasOne(v => v.Product)
                  .WithMany(p => p.Variants)
                  .HasForeignKey(v => v.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Mapeo del diccionario Specs a columna JSONB de Postgres
            entity.OwnsOne(v => v.Specs, builder => {
                builder.ToJson(); 
            });
        });
        builder.Entity<ProductImage>(entity =>
        {
            entity.HasOne(i => i.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(i => i.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<Category>()
           .HasOne(c => c.Parent)
           .WithMany(c => c.SubCategories)
           .HasForeignKey(c => c.ParentId)
           .OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Brand>(entity =>
        {
            entity.HasIndex(b => b.Name).IsUnique(); // No puede haber 2 marcas con el mismo nombre
        });
    }

    // Y no olvides exponer las tablas nuevas:
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Brand> Brands { get; set; }
}