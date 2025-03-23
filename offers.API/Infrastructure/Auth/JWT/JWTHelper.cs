using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using offers.Domain.Enums;
using offers.Domain.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace offers.API.Infrastructure.Auth.JWT
{
    public class JWTHelper
    {
        public static string GenerateSecurityToken(string Email, int Id, AccountRole Role, IOptions<JWTConfiguration> options)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(options.Value.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Email, Email),
                    new Claim(ClaimTypes.Role, nameof(Role)),
                    new Claim("id", Id.ToString())
                }),

                Expires = DateTime.UtcNow.AddMinutes(options.Value.ExpirationInMInutes),
                Audience = "localhost",
                Issuer = "localhost",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
