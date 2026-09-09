using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Infrastructure.Data;
using ClyvoVet.API.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.Tests.Unit
{
    public class ConsultaRepositoryTest
    {
        [Fact]
        [Trait("Repository", "Consultas")]
        public async Task ObterPorPetAsync_PetComConsulta_DeveRetornarConsulta()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationContext>().UseInMemoryDatabase($"Consulta-{Guid.NewGuid()}").Options;
            await using var context = new ApplicationContext(options);
            context.Tutores.Add(new Tutor { IdTutor = 1, Nome = "Ana", Email = "ana@example.com", Cpf = "104.332.181-00", Senha = "123456" });
            context.Pets.Add(new Pet { IdPet = 1, IdTutor = 1, Nome = "Thor", Especie = "Cachorro" });
            context.Consultas.Add(new Consulta { IdConsulta = 1, IdPet = 1, DataConsulta = DateTime.Today });
            await context.SaveChangesAsync();
            var repository = new ConsultaRepository(context);

            // Act
            var resultado = await repository.ObterPorPetAsync(1);

            // Assert
            Assert.Single(resultado);
        }
    }
}
