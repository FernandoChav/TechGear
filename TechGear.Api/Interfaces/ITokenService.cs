using TechGear.Api.Entities;

namespace TechGear.Api.Interfaces;

public interface ITokenService
{
    Task<string> CreateToken(User user);
}