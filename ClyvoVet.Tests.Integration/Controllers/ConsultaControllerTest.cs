using System.Net;
using System.Net.Http.Json;
using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;
using Moq;

namespace ClyvoVet.Tests.Integration
{
    public class ConsultaControllerTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        public ConsultaControllerTest(CustomWebApplicationFactory factory) => _factory = factory;

        [Fact]
        [Trait("Controller", "Consultas")]
        public async Task Post_ConsultaValida_DeveRetornarCreated()
        {
            // Arrange
            var request = new ConsultaRequestDto { IdConsulta = 1, IdPet = 1, DataConsulta = new DateTime(2026, 1, 10) };
            _factory.ConsultaUseCaseMock.Reset();
            _factory.ConsultaUseCaseMock.Setup(x => x.AdicionarConsultaAsync(It.IsAny<ConsultaRequestDto>())).ReturnsAsync(new Consulta { IdConsulta = 1, IdPet = 1, DataConsulta = request.DataConsulta });
            using var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/consultas", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Consultas")]
        public async Task Get_ConsultaExistente_DeveRetornarOK()
        {
            // Arrange
            _factory.ConsultaUseCaseMock.Reset();
            _factory.ConsultaUseCaseMock.Setup(x => x.ObterUmaConsultaAsync(1)).ReturnsAsync(new Consulta { IdConsulta = 1, IdPet = 1, DataConsulta = DateTime.Today });
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/consultas/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Consultas")]
        public async Task Get_SemConsultasCadastradas_DeveRetornarNoContent()
        {
            // Arrange
            _factory.ConsultaUseCaseMock.Reset();
            _factory.ConsultaUseCaseMock.Setup(x => x.ObterTodasConsultasAsync()).ReturnsAsync(Array.Empty<Consulta>());
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/consultas");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
