using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Infrastructure.Data;
using ClyvoVet.API.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.Tests.Unit
{
    public class MedicacaoRepositoryTest
    {
        [Fact]
        [Trait("Repository", "Medicacoes")]
        public async Task ObterPorPetAsync_PetComMedicacao_DeveRetornarMedicacao()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationContext>().UseInMemoryDatabase($"Medicacao-{Guid.NewGuid()}").Options;
            await using var context = new ApplicationContext(options);
            context.Tutores.Add(new Tutor { IdTutor = 1, Nome = "Ana", Email = "ana@example.com", Cpf = "104.332.181-00", Senha = "123456" });
            context.Pets.Add(new Pet { IdPet = 1, IdTutor = 1, Nome = "Thor", Especie = "Cachorro" });
            context.Medicacoes.Add(new Medicacao { IdMedicacao = 1, IdPet = 1, Nome = "Vermifugo" });
            await context.SaveChangesAsync();
            var repository = new MedicacaoRepository(context);

            // Act
            var resultado = await repository.ObterPorPetAsync(1);

            // Assert
            Assert.Single(resultado);
        }
    }
}
