using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DigitalNoticeBoard.Application.Abstractions;
using DigitalNoticeBoard.Application.Authentication;
using DigitalNoticeBoard.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace DigitalNoticeBoard.Api.Authentication;

public sealed class JwtTokenService(JwtSettings settings) : ITokenService
{
    public TokenResult Create(AppUser user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("display_name", user.DisplayName)
        };
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            settings.Issuer,
            settings.Audience,
            claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new TokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc);
    }
}
