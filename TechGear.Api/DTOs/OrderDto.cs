using System.ComponentModel.DataAnnotations;
using TechGear.Api.Entities; // Para acceder a ShippingAddress

namespace TechGear.Api.DTOs;

public class OrderDto
{
    [Required]
    public string BasketId { get; set; } = string.Empty;

    [Required]
    public ShippingAddressDto ShipToAddress { get; set; } = null!;
}

// DTO para la dirección (para no exponer la entidad directa)
public class ShippingAddressDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public string Street { get; set; } = string.Empty;
    [Required]
    public string City { get; set; } = string.Empty;
    [Required]
    public string State { get; set; } = string.Empty;
    [Required]
    public string ZipCode { get; set; } = string.Empty;
    [Required]
    public string Country { get; set; } = string.Empty;
}