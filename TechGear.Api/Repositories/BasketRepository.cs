using System.Text.Json;
using StackExchange.Redis;
using TechGear.Api.Entities;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Repositories;

public class BasketRepository(IConnectionMultiplexer redis) : IBasketRepository
{
    private readonly IDatabase _database = redis.GetDatabase();

    public async Task<CustomerBasket?> GetBasketAsync(string basketId)
    {
        var data = await _database.StringGetAsync(basketId);

        // CORRECCIÓN AQUÍ:
        // Usamos data.ToString() para asegurar que pasamos un string al deserializador
        return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<CustomerBasket>(data.ToString());
    }

    public async Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket)
    {
        // Serializamos el objeto a String JSON
        var json = JsonSerializer.Serialize(basket);

        // Guardamos en Redis con una vida útil de 30 días
        var created = await _database.StringSetAsync(basket.Id, json, TimeSpan.FromDays(30));

        if (!created) return null;

        return await GetBasketAsync(basket.Id);
    }

    public async Task<bool> DeleteBasketAsync(string basketId)
    {
        return await _database.KeyDeleteAsync(basketId);
    }
}