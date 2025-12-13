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
    public async Task<ActionResult<ApiResponse<UserDto>>> Login(LoginDto loginDto)
    {
        // 1. Validaciones (Igual que tenías)
        var user = await _userManager.FindByEmailAsync(loginDto.Email);
        if (user == null) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Email o contraseña inválidos" });

        var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!result) return Unauthorized(new ApiResponse<string> { Success = false, Message = "Email o contraseña inválidos" });

        // 2. Generar Token y Cookie (Igual que tenías)
        var token = await _tokenService.CreateToken(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("accessToken", token, cookieOptions);

        // 3. PREPARAR LOS DATOS DEL USUARIO (Esto es lo nuevo)
        // Obtenemos los roles para enviarlos al frontend
        var roles = await _userManager.GetRolesAsync(user);

        var userDto = new UserDto
        {
            Email = user.Email!,
            FullName = user.FullName,
            Roles = [.. roles] // Sintaxis de colección de C# 12
        };

        // 4. RETORNAR API RESPONSE CON DATOS
        // El frontend recibe "Success: true" y los datos del usuario para actualizar el estado global
        return Ok(new ApiResponse<UserDto>
        {
            Success = true,
            Message = "Login exitoso. Bienvenido a TechGear.",
            Data = userDto
        });
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
    [HttpPost("logout")]
    public ActionResult<ApiResponse<string>> Logout()
    {
        // 1. Crear las opciones de la cookie (Deben coincidir con las del Login)
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            // 2. La clave: poner la fecha de expiración en el pasado
            Expires = DateTime.UtcNow.AddDays(-1)
        };

        // 3. Sobrescribir la cookie "accessToken" con un valor vacío
        Response.Cookies.Append("accessToken", "", cookieOptions);

        // 4. Retornar respuesta exitosa
        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Has cerrado sesión correctamente."
        });
    }
}