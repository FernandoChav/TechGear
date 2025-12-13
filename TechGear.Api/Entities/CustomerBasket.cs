namespace TechGear.Api.Entities;

public class CustomerBasket
{
    // El ID será generado por el Frontend (un GUID aleatorio) o el ID del User si está logueado
    public string Id { get; set; } 
    public List<BasketItem> Items { get; set; } = [];

    // Constructor vacío requerido por el serializador
    public CustomerBasket() { Id = string.Empty; }

    public CustomerBasket(string id)
    {
        Id = id;
    }
}