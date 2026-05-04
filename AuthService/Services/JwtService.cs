using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using AuthService.Models;

namespace AuthService.Services;

public class JwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user)
    {
        // 🔥 Map RoleId → Role Name (SAFE & CLEAN)
        var role = user.RoleId switch
        {
            1 => "Admin",
            2 => "Cashier",
            _ => "User"
        };

        // ✅ FINAL CLAIMS (NO DUPLICATE ROLE)
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), // user id
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty),  // email
            new Claim(ClaimTypes.Role, role)                          // role
        };

        // 🔐 KEY
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is missing"))
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 🎟 TOKEN
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "RetailPOS",
            audience: _config["Jwt:Audience"] ?? "RetailPOSUsers",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
