using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.UseCases;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;
using Moq;

namespace ClyvoVet.Tests.Unit
{
    public class TutorUseCaseTest
    {
        private readonly Mock<ITutorRepository> _repository = new();

        [Fact]
        [Trait("Application", "Tutores")]
        public async Task ObterUmTutorAsync_TutorExistente_DeveRetornarTutor()
        {
            // Arrange
            _repository.Setup(x => x.ObterUmAsync(1)).ReturnsAsync(new Tutor
            {
                IdTutor = 1,
                Nome = "Ana",
                Email = "ana@example.com",
                Cpf = "104.332.181-00",
                Senha = "123456"
            });
            var useCase = new TutorUseCase(_repository.Object);

            // Act
            var resultado = await useCase.ObterUmTutorAsync(1);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(1, resultado.IdTutor);
        }

        [Fact]
        [Trait("Application", "Tutores")]
        public async Task AdicionarTutorAsync_EmailNovo_DeveCadastrarTutor()
        {
            // Arrange
            var dto = new TutorRequestDto
            {
                IdTutor = 5,
                Nome = "Carla",
                Email = "carla@example.com",
                Cpf = "083.863.794-99",
                Senha = "123456"
            };
            _repository.Setup(x => x.ExisteEmailAsync(dto.Email)).ReturnsAsync(false);
            _repository.Setup(x => x.AdicionarAsync(It.IsAny<Tutor>())).ReturnsAsync((Tutor x) => x);
            var useCase = new TutorUseCase(_repository.Object);

            // Act
            var resultado = await useCase.AdicionarTutorAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(5, resultado.IdTutor);
            Assert.Equal(dto.Senha, resultado.Senha);
            _repository.Verify(x => x.AdicionarAsync(It.Is<Tutor>(t => t.Nome == "Carla")), Times.Once);
        }

        [Fact]
        [Trait("Application", "Tutores")]
        public async Task AdicionarTutorAsync_EmailExistente_DeveRetornarNull()
        {
            // Arrange
            var dto = new TutorRequestDto
            {
                IdTutor = 5,
                Nome = "Carla",
                Email = "carla@example.com",
                Cpf = "083.863.794-99",
                Senha = "123456"
            };
            _repository.Setup(x => x.ExisteEmailAsync(dto.Email)).ReturnsAsync(true);
            var useCase = new TutorUseCase(_repository.Object);

            // Act
            var resultado = await useCase.AdicionarTutorAsync(dto);

            // Assert
            Assert.Null(resultado);
            _repository.Verify(x => x.AdicionarAsync(It.IsAny<Tutor>()), Times.Never);
        }

        [Fact]
        [Trait("Application", "Tutores")]
        public async Task AutenticarTutorAsync_CredenciaisValidas_DeveRetornarTutor()
        {
            // Arrange
            var dto = new LoginRequestDto { Email = "ana@teste.com", Senha = "123456" };
            var tutor = new Tutor
            {
                IdTutor = 1,
                Nome = "Ana",
                Email = dto.Email,
                Cpf = "104.332.181-00",
                Senha = dto.Senha
            };
            _repository.Setup(x => x.ObterPorCredenciaisAsync(dto.Email, dto.Senha)).ReturnsAsync(tutor);
            var useCase = new TutorUseCase(_repository.Object);

            // Act
            var resultado = await useCase.AutenticarTutorAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(dto.Email, resultado.Email);
        }
    }
}
