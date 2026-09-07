using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.UseCases;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;
using Moq;

namespace ClyvoVet.Tests.Unit
{
    public class UserUseCaseTest
    {
        private readonly Mock<IUserRepository> _userRepository;
        private readonly UserUseCase _userUseCase;

        public UserUseCaseTest()
        {
            _userRepository = new Mock<IUserRepository>();
            _userUseCase = new UserUseCase(_userRepository.Object);
        }

        [Fact]
        [Trait("UseCase", "Users")]
        public async Task RegistrarUsuarioAsync_EmailNovo_DeveCadastrarUsuario()
        {
            // Arrange
            var dto = new UserRequestDto
            {
                Name = "Ana",
                Email = "ana@teste.com",
                Password = "123456",
                Phone = "11999999999",
                Address = "São Paulo"
            };

            _userRepository.Setup(obj => obj.ExisteEmailAsync(dto.Email)).ReturnsAsync(false);
            _userRepository
                .Setup(obj => obj.AdicionarAsync(It.IsAny<User>()))
                .ReturnsAsync((User entity) => entity);

            // Act
            var resultado = await _userUseCase.RegistrarUsuarioAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(dto.Email, resultado.Email);
            _userRepository.Verify(obj => obj.AdicionarAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        [Trait("UseCase", "Users")]
        public async Task RegistrarUsuarioAsync_EmailExistente_DeveRetornarNull()
        {
            // Arrange
            var dto = new UserRequestDto
            {
                Name = "Ana",
                Email = "ana@teste.com",
                Password = "123456"
            };
            _userRepository.Setup(obj => obj.ExisteEmailAsync(dto.Email)).ReturnsAsync(true);

            // Act
            var resultado = await _userUseCase.RegistrarUsuarioAsync(dto);

            // Assert
            Assert.Null(resultado);
            _userRepository.Verify(obj => obj.AdicionarAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        [Trait("UseCase", "Users")]
        public async Task AutenticarUsuarioAsync_CredenciaisValidas_DeveRetornarUsuario()
        {
            // Arrange
            var dto = new LoginRequestDto { Email = "ana@teste.com", Password = "123456" };
            var user = new User { Email = dto.Email, Password = dto.Password };
            _userRepository.Setup(obj => obj.ObterPorCredenciaisAsync(dto.Email, dto.Password)).ReturnsAsync(user);

            // Act
            var resultado = await _userUseCase.AutenticarUsuarioAsync(dto);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(dto.Email, resultado.Email);
        }
    }
}
