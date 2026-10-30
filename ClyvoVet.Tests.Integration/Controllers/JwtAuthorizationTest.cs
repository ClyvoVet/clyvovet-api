using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace ClyvoVet.Tests.Integration
{
    public class JwtAuthorizationTest
    {
        private const string Secret = "ClyvoVet_Sprint4_FIAP_2026_Chave_JWT_Segura_123456";

        [Fact]
        [Trait("Security", "JWT")]
        public async Task GetPets_SemToken_DeveRetornarUnauthorized()
        {
            // Arrange
            await using var factory = CriarFactory();
            using var client = factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/pets");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        [Trait("Security", "JWT")]
        public async Task GetPets_TokenSemRoleTutor_DeveRetornarForbidden()
        {
            // Arrange
            await using var factory = CriarFactory();
            using var client = factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GerarToken("guest"));

            // Act
            var response = await client.GetAsync("/api/pets");

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static WebApplicationFactory<Program> CriarFactory()
        {
            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.UseEnvironment("Testing");
                    builder.ConfigureAppConfiguration((_, configuration) =>
                    {
                        configuration.AddInMemoryCollection(new Dictionary<string, string?>
                        {
                            ["Secretkey"] = Secret,
                            ["Observability:EnableConsoleExporter"] = "false"
                        });
                    });
                });
        }

        private static string GerarToken(string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
            var token = new JwtSecurityToken(
                issuer: "ClyvoVet.API",
                audience: "ClyvoVet.Clients",
                claims: [new Claim(ClaimTypes.Name, "teste@clyvovet.local"), new Claim(ClaimTypes.Role, role)],
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
