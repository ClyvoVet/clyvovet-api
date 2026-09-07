using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Infrastructure.Data;
using ClyvoVet.API.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.Tests.Unit
{
    public class PetRepositoryTest
    {
        private readonly ApplicationContext _applicationContext;
        private readonly PetRepository _petRepository;

        public PetRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: $"PetRepositoryTest-{Guid.NewGuid()}")
                .Options;

            _applicationContext = new ApplicationContext(options);
            _applicationContext.Database.EnsureCreated();
            _petRepository = new PetRepository(_applicationContext);
        }

        [Fact]
        [Trait("Repository", "Pets")]
        public async Task ObterTodosAsync_ComPetsCadastrados_DeveRetornarPets()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            _applicationContext.Pets.AddRange(
                new Pet { Name = "Luna", Species = "Cachorro", OwnerId = ownerId },
                new Pet { Name = "Mia", Species = "Gato", OwnerId = ownerId });
            await _applicationContext.SaveChangesAsync();

            // Act
            var resultado = await _petRepository.ObterTodosAsync();

            // Assert
            Assert.Equal(2, resultado.Count());
        }

        [Fact]
        [Trait("Repository", "Pets")]
        public async Task ObterUmAsync_PetExistente_DeveRetornarPet()
        {
            // Arrange
            var pet = new Pet { Name = "Luna", Species = "Cachorro", OwnerId = Guid.NewGuid() };
            _applicationContext.Pets.Add(pet);
            await _applicationContext.SaveChangesAsync();

            // Act
            var resultado = await _petRepository.ObterUmAsync(pet.Id);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(pet.Id, resultado.Id);
        }

        [Fact]
        [Trait("Repository", "Pets")]
        public async Task AdicionarAsync_PetValido_DevePersistirPet()
        {
            // Arrange
            var pet = new Pet { Name = "Thor", Species = "Cachorro", OwnerId = Guid.NewGuid() };

            // Act
            var resultado = await _petRepository.AdicionarAsync(pet);

            // Assert
            var petNoDb = await _applicationContext.Pets.FirstOrDefaultAsync(x => x.Id == resultado.Id);
            Assert.NotNull(petNoDb);
            Assert.Equal("Thor", petNoDb.Name);
        }
    }
}
