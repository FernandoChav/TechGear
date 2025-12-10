using System.ComponentModel.DataAnnotations;

namespace TechGear.Api.Entities;

public class Brand : BaseAuditableEntity
{
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Slug { get; set; } = string.Empty;
}