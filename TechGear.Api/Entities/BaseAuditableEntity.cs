using System.ComponentModel.DataAnnotations;

namespace TechGear.Api.Entities;

public abstract class BaseAuditableEntity
{
    [Key]
    public Guid Id { get; set; } 

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastModifiedAt { get; set; }

    public bool IsDeleted { get; set; } = false;
}