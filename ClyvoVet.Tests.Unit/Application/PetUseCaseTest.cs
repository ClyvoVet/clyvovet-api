using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.UseCases;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;
using Moq;

namespace ClyvoVet.Tests.Unit
{
    public class PetUseCaseTest
    {
        private readonly Mock<IPetRepository> _petRepository;
        private readonly PetUseCase _petUseCase;

        public PetUseCaseTest()
        {
            _petRepository = new Mock<IPetRepository>();
            _petUseCase = new PetUseCase(_petRepository.Object);
        }

        [Fact]
        [Trait("UseCase", "Pets")]
        public async Task ObterUmPetAsync_PetExistente_DeveRetornarPet()
        {
            // Arrange
            var id = Guid.NewGuid();
            var pet = new Pet { Id = id, Name = "Luna", Species = "Cachorro" };
            _petRepository.Setup(obj => obj.ObterUmAsync(id)).ReturnsAsync(pet);

            // Act
            var resultado = await _petUseCase.ObterUmPetAsync(id);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(id, resultado.Id);
            Assert.Equal("Luna", resultado.Name);
        }

        [Fact]
        [Trait("UseCase", "Pets")]
        public async Task AdicionarPetAsync_DadosValidos_DeveRetornarPetCriado()
        {
            // Arrange
            var ownerId = Guid.NewGuid();
            var dto = new PetRequestDto
            {
                Name = "Thor",
                Species = "Cachorro",
                Breed = "SRD",
                Weight = 15,
                Color = "Caramelo",
                NextCheckup = DateTime.UtcNow.AddDays(30),
                OwnerId = ownerId
            };

            _petRepository
                .Setup(obj => obj.AdicionarAsync(It.IsAny<Pet>()))
                .ReturnsAsync((Pet entity) => entity);

            // Act
            var resultado = await _petUseCase.AdicionarPetAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(dto.Name, resultado.Name);
            Assert.Equal(ownerId, resultado.OwnerId);
            _petRepository.Verify(obj => obj.AdicionarAsync(It.IsAny<Pet>()), Times.Once);
        }

        [Fact]
        [Trait("UseCase", "Pets")]
        public async Task DeletarPetAsync_PetInexistente_DeveRetornarNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _petRepository.Setup(obj => obj.DeletarAsync(id)).ReturnsAsync((Pet?)null);

            // Act
            var resultado = await _petUseCase.DeletarPetAsync(id);

            // Assert
            Assert.Null(resultado);
        }
    }
}
