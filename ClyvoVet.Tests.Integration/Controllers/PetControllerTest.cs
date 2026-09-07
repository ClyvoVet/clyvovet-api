using System.Net;
using ClyvoVet.API.Domain.Entities;
using Moq;

namespace ClyvoVet.Tests.Integration
{
    public class PetControllerTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public PetControllerTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        [Trait("Controller", "Pets")]
        public async Task Get_SemPetsCadastrados_DeveRetornarNoContent()
        {
            // Arrange
            _factory.PetUseCaseMock.Reset();
            _factory.PetUseCaseMock
                .Setup(x => x.ObterTodosPetsAsync())
                .ReturnsAsync(Array.Empty<Pet>());
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/pets");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Pets")]
        public async Task Get_PetExistente_DeveRetornarOK()
        {
            // Arrange
            var id = Guid.NewGuid();
            var pet = new Pet { Id = id, Name = "Luna", Species = "Cachorro" };
            _factory.PetUseCaseMock.Reset();
            _factory.PetUseCaseMock.Setup(x => x.ObterUmPetAsync(id)).ReturnsAsync(pet);
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync($"api/pets/{id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Pets")]
        public async Task Get_PetInexistente_DeveRetornarNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            _factory.PetUseCaseMock.Reset();
            _factory.PetUseCaseMock.Setup(x => x.ObterUmPetAsync(id)).ReturnsAsync((Pet?)null);
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync($"api/pets/{id}");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
