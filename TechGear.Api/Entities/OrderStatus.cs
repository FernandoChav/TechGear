using System.Runtime.Serialization;

namespace TechGear.Api.Entities;

public enum OrderStatus
{
    [EnumMember(Value = "Pending")]
    Pending,

    [EnumMember(Value = "PaymentReceived")]
    PaymentReceived,

    [EnumMember(Value = "PaymentFailed")]
    PaymentFailed,
    
    [EnumMember(Value = "Shipped")] // Enviado
    Shipped
}