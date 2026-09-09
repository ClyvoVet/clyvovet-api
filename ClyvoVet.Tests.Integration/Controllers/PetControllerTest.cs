using System.Net;
using System.Net.Http.Json;
using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;
using Moq;

namespace ClyvoVet.Tests.Integration
{
    public class PetControllerTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        public PetControllerTest(CustomWebApplicationFactory factory) => _factory = factory;

        [Fact]
        [Trait("Controller", "Pets")]
        public async Task Post_PetValido_DeveRetornarCreated()
        {
            // Arrange
            var request = new PetRequestDto { IdPet = 1, IdTutor = 1, Nome = "Thor", Especie = "Cachorro", PesoKg = 28.50m };
            _factory.PetUseCaseMock.Reset();
            _factory.PetUseCaseMock.Setup(x => x.AdicionarPetAsync(It.IsAny<PetRequestDto>())).ReturnsAsync(new Pet { IdPet = 1, IdTutor = 1, Nome = "Thor", Especie = "Cachorro", PesoKg = 28.50m });
            using var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/pets", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Pets")]
        public async Task Get_SemPetsCadastrados_DeveRetornarNoContent()
        {
            // Arrange
            _factory.PetUseCaseMock.Reset();
            _factory.PetUseCaseMock.Setup(x => x.ObterTodosPetsAsync()).ReturnsAsync(Array.Empty<Pet>());
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
            _factory.PetUseCaseMock.Reset();
            _factory.PetUseCaseMock.Setup(x => x.ObterUmPetAsync(1)).ReturnsAsync(new Pet { IdPet = 1, IdTutor = 1, Nome = "Thor", Especie = "Cachorro" });
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/pets/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Pets")]
        public async Task Get_PetInexistente_DeveRetornarNotFound()
        {
            // Arrange
            _factory.PetUseCaseMock.Reset();
            _factory.PetUseCaseMock.Setup(x => x.ObterUmPetAsync(999)).ReturnsAsync((Pet?)null);
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/pets/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
