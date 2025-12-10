using Microsoft.AspNetCore.Identity;

namespace TechGear.Api.Entities;

public class User : IdentityUser
{
    
    public string FullName { get; set; } = string.Empty;
    

}