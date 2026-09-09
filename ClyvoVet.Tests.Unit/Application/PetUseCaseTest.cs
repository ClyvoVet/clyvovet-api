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
            var pet = new Pet { IdPet = 1, IdTutor = 1, Nome = "Luna", Especie = "Cachorro" };
            _petRepository.Setup(x => x.ObterUmAsync(1)).ReturnsAsync(pet);

            // Act
            var resultado = await _petUseCase.ObterUmPetAsync(1);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.IdPet);
            Assert.Equal("Luna", resultado.Nome);
        }

        [Fact]
        [Trait("UseCase", "Pets")]
        public async Task AdicionarPetAsync_DadosValidos_DeveRetornarPetCriado()
        {
            // Arrange
            var dto = new PetRequestDto
            {
                IdPet = 2,
                IdTutor = 1,
                Nome = "Thor",
                Especie = "Cachorro",
                Raca = "Labrador",
                DataNascimento = new DateTime(2021, 3, 10),
                PesoKg = 28.50m
            };
            _petRepository.Setup(x => x.AdicionarAsync(It.IsAny<Pet>())).ReturnsAsync((Pet x) => x);

            // Act
            var resultado = await _petUseCase.AdicionarPetAsync(dto);

            // Assert
            Assert.Equal(dto.IdPet, resultado.IdPet);
            Assert.Equal(dto.IdTutor, resultado.IdTutor);
            Assert.Equal(dto.PesoKg, resultado.PesoKg);
            _petRepository.Verify(x => x.AdicionarAsync(It.IsAny<Pet>()), Times.Once);
        }

        [Fact]
        [Trait("UseCase", "Pets")]
        public async Task ObterPetsPorTutorAsync_TutorComPets_DeveRetornarPets()
        {
            // Arrange
            _petRepository.Setup(x => x.ObterPorTutorAsync(1)).ReturnsAsync(new[] { new Pet { IdPet = 1, IdTutor = 1, Nome = "Thor", Especie = "Cachorro" } });

            // Act
            var resultado = await _petUseCase.ObterPetsPorTutorAsync(1);

            // Assert
            Assert.Single(resultado);
        }

        [Fact]
        [Trait("UseCase", "Pets")]
        public async Task DeletarPetAsync_PetInexistente_DeveRetornarNull()
        {
            // Arrange
            _petRepository.Setup(x => x.DeletarAsync(999)).ReturnsAsync((Pet?)null);

            // Act
            var resultado = await _petUseCase.DeletarPetAsync(999);

            // Assert
            Assert.Null(resultado);
        }
    }
}
