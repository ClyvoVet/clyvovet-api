using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;
using ClyvoVet.API.Domain.Models;
using Microsoft.IdentityModel.Tokens;

namespace ClyvoVet.API.Infrastructure.Security
{
    public class JwtTokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public TokenResult GerarToken(Tutor tutor)
        {
            var secret = _configuration["Secretkey"];
            if (string.IsNullOrWhiteSpace(secret))
                throw new InvalidOperationException("Secretkey não foi configurada.");

            var expiresAtUtc = DateTime.UtcNow.AddHours(8);
            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, tutor.IdTutor.ToString()),
                new Claim(ClaimTypes.Name, tutor.Email),
                new Claim(ClaimTypes.Email, tutor.Email),
                new Claim(ClaimTypes.Role, "tutor")
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature));

            return new TokenResult
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAtUtc = expiresAtUtc
            };
        }
    }
}
