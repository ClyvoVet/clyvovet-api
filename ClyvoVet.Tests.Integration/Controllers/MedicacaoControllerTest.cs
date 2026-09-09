using System.Net;
using System.Net.Http.Json;
using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;
using Moq;

namespace ClyvoVet.Tests.Integration
{
    public class MedicacaoControllerTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        public MedicacaoControllerTest(CustomWebApplicationFactory factory) => _factory = factory;

        [Fact]
        [Trait("Controller", "Medicacoes")]
        public async Task Post_MedicacaoValida_DeveRetornarCreated()
        {
            // Arrange
            var request = new MedicacaoRequestDto { IdMedicacao = 1, IdPet = 1, Nome = "Vermifugo" };
            _factory.MedicacaoUseCaseMock.Reset();
            _factory.MedicacaoUseCaseMock.Setup(x => x.AdicionarMedicacaoAsync(It.IsAny<MedicacaoRequestDto>())).ReturnsAsync(new Medicacao { IdMedicacao = 1, IdPet = 1, Nome = request.Nome });
            using var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/medicacoes", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Medicacoes")]
        public async Task Get_MedicacaoExistente_DeveRetornarOK()
        {
            // Arrange
            _factory.MedicacaoUseCaseMock.Reset();
            _factory.MedicacaoUseCaseMock.Setup(x => x.ObterUmaMedicacaoAsync(1)).ReturnsAsync(new Medicacao { IdMedicacao = 1, IdPet = 1, Nome = "Vermifugo" });
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/medicacoes/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Medicacoes")]
        public async Task Get_MedicacaoInexistente_DeveRetornarNotFound()
        {
            // Arrange
            _factory.MedicacaoUseCaseMock.Reset();
            _factory.MedicacaoUseCaseMock.Setup(x => x.ObterUmaMedicacaoAsync(999)).ReturnsAsync((Medicacao?)null);
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/medicacoes/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
