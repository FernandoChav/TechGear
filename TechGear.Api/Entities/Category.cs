using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechGear.Api.Entities;

public class Category : BaseAuditableEntity
{
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Slug { get; set; } = string.Empty;

    // Jerarquía (Self-Referencing): Una categoría puede tener un padre
    public Guid? ParentId { get; set; }
    
    [ForeignKey("ParentId")]
    public Category? Parent { get; set; }

    // Una categoría puede tener muchas subcategorías hijas
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
}