using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Infrastructure.Data;
using ClyvoVet.API.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.Tests.Unit
{
    public class PetRepositoryTest
    {
        private readonly ApplicationContext _context;
        private readonly PetRepository _repository;

        public PetRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase($"PetRepositoryTest-{Guid.NewGuid()}")
                .Options;
            _context = new ApplicationContext(options);
            _context.Database.EnsureCreated();
            _repository = new PetRepository(_context);
        }

        private async Task AdicionarTutorAsync(int id = 1)
        {
            _context.Tutores.Add(new Tutor { IdTutor = id, Nome = $"Tutor {id}", Email = $"tutor{id}@example.com", Cpf = "104.332.181-00", Senha = "123456" });
            await _context.SaveChangesAsync();
        }

        [Fact]
        [Trait("Repository", "Pets")]
        public async Task ObterTodosAsync_ComPetsCadastrados_DeveRetornarPets()
        {
            // Arrange
            await AdicionarTutorAsync();
            _context.Pets.AddRange(
                new Pet { IdPet = 1, IdTutor = 1, Nome = "Luna", Especie = "Cachorro" },
                new Pet { IdPet = 2, IdTutor = 1, Nome = "Mia", Especie = "Gato" });
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.ObterTodosAsync();

            // Assert
            Assert.Equal(2, resultado.Count());
        }

        [Fact]
        [Trait("Repository", "Pets")]
        public async Task ObterUmAsync_PetExistente_DeveRetornarPet()
        {
            // Arrange
            await AdicionarTutorAsync();
            _context.Pets.Add(new Pet { IdPet = 1, IdTutor = 1, Nome = "Luna", Especie = "Cachorro" });
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.ObterUmAsync(1);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.IdPet);
        }

        [Fact]
        [Trait("Repository", "Pets")]
        public async Task ObterPorTutorAsync_TutorComPets_DeveRetornarSomentePetsDoTutor()
        {
            // Arrange
            await AdicionarTutorAsync(1);
            await AdicionarTutorAsync(2);
            _context.Pets.AddRange(
                new Pet { IdPet = 1, IdTutor = 1, Nome = "Thor", Especie = "Cachorro" },
                new Pet { IdPet = 2, IdTutor = 2, Nome = "Mel", Especie = "Cachorro" });
            await _context.SaveChangesAsync();

            // Act
            var resultado = await _repository.ObterPorTutorAsync(1);

            // Assert
            Assert.Single(resultado);
            Assert.Equal(1, resultado.Single().IdTutor);
        }

        [Fact]
        [Trait("Repository", "Pets")]
        public async Task AdicionarAsync_PetValido_DevePersistirPet()
        {
            // Arrange
            await AdicionarTutorAsync();
            var pet = new Pet { IdPet = 3, IdTutor = 1, Nome = "Thor", Especie = "Cachorro" };

            // Act
            var resultado = await _repository.AdicionarAsync(pet);

            // Assert
            var petNoDb = await _context.Pets.FirstOrDefaultAsync(x => x.IdPet == resultado.IdPet);
            Assert.NotNull(petNoDb);
            Assert.Equal("Thor", petNoDb.Nome);
        }
    }
}
