using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.UseCases;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;
using Moq;

namespace ClyvoVet.Tests.Unit
{
    public class MedicacaoUseCaseTest
    {
        [Fact]
        [Trait("Application", "Medicacoes")]
        public async Task AdicionarMedicacaoAsync_DadosValidos_DeveEnviarEntidadeAoRepository()
        {
            // Arrange
            var repository = new Mock<IMedicacaoRepository>();
            repository.Setup(x => x.AdicionarAsync(It.IsAny<Medicacao>())).ReturnsAsync((Medicacao x) => x);
            var useCase = new MedicacaoUseCase(repository.Object);
            var dto = new MedicacaoRequestDto { IdMedicacao = 1, IdPet = 3, Nome = "Cetoconazol", Dose = "1 comprimido" };

            // Act
            var resultado = await useCase.AdicionarMedicacaoAsync(dto);

            // Assert
            Assert.Equal("Cetoconazol", resultado.Nome);
            Assert.Equal(3, resultado.IdPet);
        }

        [Fact]
        [Trait("Application", "Medicacoes")]
        public async Task ObterMedicacoesPorPetAsync_PetComMedicacoes_DeveRetornarMedicacoes()
        {
            // Arrange
            var repository = new Mock<IMedicacaoRepository>();
            repository.Setup(x => x.ObterPorPetAsync(3)).ReturnsAsync(new[] { new Medicacao { IdMedicacao = 1, IdPet = 3, Nome = "Cetoconazol" } });
            var useCase = new MedicacaoUseCase(repository.Object);

            // Act
            var resultado = await useCase.ObterMedicacoesPorPetAsync(3);

            // Assert
            Assert.Single(resultado);
        }
    }
}
