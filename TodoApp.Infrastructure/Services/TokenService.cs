using System;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices.Marshalling;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using TodoApp.Api.Telemetry;
using TodoApp.Application.Services.Interfaces;
using TodoApp.Domain;
using TodoApp.Telemetry.Attributes;

namespace TodoApp.Infrastructure.Services;

[Telemetry]
public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GenerateToken(User user)
    {
        var keyString = configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key not found");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}