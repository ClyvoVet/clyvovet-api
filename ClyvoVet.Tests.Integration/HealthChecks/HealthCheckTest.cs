using System.Net;

namespace ClyvoVet.Tests.Integration
{
    [Collection("API Integration")]
    public class HealthCheckTest
    {
        private readonly CustomWebApplicationFactory _factory;

        public HealthCheckTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        [Trait("HealthCheck", "Live")]
        public async Task HealthLive_ApiExecutando_DeveRetornarOK()
        {
            // Arrange
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/health/live");
            var content = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("Healthy", content);
            Assert.Contains("self", content);
        }
        [Fact]
        [Trait("HealthCheck", "Controller")]
        public async Task HealthControllerLive_ApiExecutando_DeveRetornarOK()
        {
            // Arrange
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/api/health2/live");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}
