using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TechGear.Api.Entities;
using TechGear.Api.DTOs;
using TechGear.Api.Interfaces;

namespace TechGear.Api.Controllers;

public class AccountController(UserManager<User> userManager, ITokenService tokenService) : BaseApiController
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly ITokenService _tokenService = tokenService;

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterDto registerDto)
    {
        if (await _userManager.FindByEmailAsync(registerDto.Email) != null)
        {
            return BadRequest("El correo ya está registrado.");
        }

        var user = new User
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FullName = registerDto.FullName
        };

        var result = await _userManager.CreateAsync(user, registerDto.Password);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(error.Code, error.Description);
            return ValidationProblem();
        }

        await _userManager.AddToRoleAsync(user, "Customer");
        return StatusCode(201, new { message = "Usuario registrado exitosamente" });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(LoginDto loginDto) // Nota: Ya no devuelve <UserDto>
    {
        // 1. Buscar usuario
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null) return Unauthorized("Email o contraseña inválidos");

        // 2. Verificar contraseña
        var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!result) return Unauthorized("Email o contraseña inválidos");

        // 3. Generar Token
        var token = await _tokenService.CreateToken(user);

        // 4. Configurar la Cookie HttpOnly
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,  
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("accessToken", token, cookieOptions);

        // 5. Respuesta Limpia (Como pediste)
        return Ok(new { message = "Login exitoso. Bienvenido a TechGear." });
    }
    
    // Endpoint útil para que el Frontend sepa si la cookie sigue viva
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        // User.Identity.Name viene del Claim del token que leímos de la cookie automáticamente
        var email = User.Identity?.Name;
        
        if (string.IsNullOrEmpty(email)) return Unauthorized();

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return Unauthorized();
        
        var roles = await _userManager.GetRolesAsync(user);

        return new UserDto
        {
            Email = user.Email!,
            FullName = user.FullName,
            Roles = [.. roles]
        };
    }
}