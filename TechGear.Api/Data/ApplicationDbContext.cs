using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TechGear.Api.Entities;

namespace TechGear.Api.Data;

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

        // ======================================
        // 1. CONFIGURACIÓN DE PRODUCTOS
        // ======================================
        builder.Entity<Product>(entity =>
        {
            entity.HasIndex(p => p.Slug).IsUnique(); 
            entity.HasIndex(p => p.Name); 
        });

        // 2. Configuración de ProductVariant
        builder.Entity<ProductVariant>(entity =>
        {
            entity.HasIndex(v => v.Sku).IsUnique(); 

            entity.HasOne(v => v.Product)
                  .WithMany(p => p.Variants)
                  .HasForeignKey(v => v.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Mapeo JSONB para Specs
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
            entity.HasIndex(b => b.Name).IsUnique(); 
        });

        // ======================================
        // 2. NUEVA CONFIGURACIÓN DE ÓRDENES
        // ======================================
        builder.Entity<Order>(entity =>
        {
            // A. Owned Entity: La dirección se guarda en columnas dentro de la tabla "Orders"
            // (Street, City, ZipCode...) en lugar de una tabla separada.
            entity.OwnsOne(o => o.ShipToAddress, a =>
            {
                a.WithOwner(); 
            });

            // B. Conversión de Enum a String para que sea legible en la BD
            // Pending -> "Pending"
            entity.Property(s => s.Status)
                .HasConversion(
                    o => o.ToString(),
                    o => (OrderStatus)Enum.Parse(typeof(OrderStatus), o)
                );

            // C. Borrado en Cascada: Si borras la Orden, se borran sus items
            entity.HasMany(o => o.OrderItems)
                  .WithOne()
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }

    // ======================================
    // 3. TABLAS (DbSets)
    // ======================================
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Brand> Brands { get; set; }

    // NUEVAS TABLAS
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}