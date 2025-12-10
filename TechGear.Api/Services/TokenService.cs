using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using TechGear.Api.Entities;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Services;

public class TokenService(IConfiguration config, UserManager<User> userManager) : ITokenService
{
    private readonly IConfiguration _config = config;
    private readonly UserManager<User> _userManager = userManager;

    public async Task<string> CreateToken(User user)
    {
        // 1. Definir los "Claims" (Datos dentro del token)
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.NameIdentifier, user.Id), // El ID del usuario
            new(ClaimTypes.Name, user.UserName!)
        };

        // 2. Agregar los Roles al token (Admin, Customer)
        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // 3. Crear la llave de encriptación
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:TokenKey"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        // 4. Configurar el Token
        var tokenOptions = new JwtSecurityToken(
            issuer: null,
            audience: null,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7), // Dura 7 días
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenOptions);
    }
}