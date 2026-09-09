using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.UseCases;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;
using Moq;

namespace ClyvoVet.Tests.Unit
{
    public class ConsultaUseCaseTest
    {
        [Fact]
        [Trait("Application", "Consultas")]
        public async Task AdicionarConsultaAsync_DadosValidos_DeveEnviarEntidadeAoRepository()
        {
            // Arrange
            var repository = new Mock<IConsultaRepository>();
            repository.Setup(x => x.AdicionarAsync(It.IsAny<Consulta>())).ReturnsAsync((Consulta x) => x);
            var useCase = new ConsultaUseCase(repository.Object);
            var dto = new ConsultaRequestDto { IdConsulta = 1, IdPet = 1, DataConsulta = new DateTime(2026, 1, 10), Veterinario = "Dr. Marcelo" };

            // Act
            var resultado = await useCase.AdicionarConsultaAsync(dto);

            // Assert
            Assert.Equal(1, resultado.IdConsulta);
            Assert.Equal(1, resultado.IdPet);
        }

        [Fact]
        [Trait("Application", "Consultas")]
        public async Task ObterConsultasPorPetAsync_PetComConsultas_DeveRetornarConsultas()
        {
            // Arrange
            var repository = new Mock<IConsultaRepository>();
            repository.Setup(x => x.ObterPorPetAsync(1)).ReturnsAsync(new[] { new Consulta { IdConsulta = 1, IdPet = 1, DataConsulta = DateTime.Today } });
            var useCase = new ConsultaUseCase(repository.Object);

            // Act
            var resultado = await useCase.ObterConsultasPorPetAsync(1);

            // Assert
            Assert.Single(resultado);
        }
    }
}
