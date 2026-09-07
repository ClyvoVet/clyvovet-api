using System.Net;
using System.Net.Http.Json;
using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;
using Moq;

namespace ClyvoVet.Tests.Integration
{
    [Collection("API Integration")]
    public class UserControllerTest
    {
        private readonly CustomWebApplicationFactory _factory;

        public UserControllerTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        [Trait("Controller", "Users")]
        public async Task Register_UsuarioValido_DeveRetornarCreated()
        {
            // Arrange
            var request = new UserRequestDto
            {
                Name = "Usuario Teste",
                Email = "usuario.teste@example.com",
                Password = "SenhaTeste123456",
                Phone = "11999999999",
                Address = "Rua de teste"
            };
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                Phone = request.Phone,
                Address = request.Address
            };
            _factory.UserUseCaseMock.Reset();
            _factory.UserUseCaseMock
                .Setup(x => x.RegistrarUsuarioAsync(It.IsAny<UserRequestDto>()))
                .ReturnsAsync(user);
            using var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/users/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Users")]
        public async Task Register_EmailExistente_DeveRetornarBadRequest()
        {
            // Arrange
            var request = new UserRequestDto
            {
                Name = "Usuario Teste",
                Email = "usuario.teste@example.com",
                Password = "SenhaTeste123456"
            };
            _factory.UserUseCaseMock.Reset();
            _factory.UserUseCaseMock
                .Setup(x => x.RegistrarUsuarioAsync(It.IsAny<UserRequestDto>()))
                .ReturnsAsync((User?)null);
            using var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/users/register", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Users")]
        public async Task Login_CredenciaisValidas_DeveRetornarOK()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "ana@teste.com", Password = "123456" };
            var user = new User { Email = request.Email, Password = request.Password, Name = "Ana" };
            _factory.UserUseCaseMock.Reset();
            _factory.UserUseCaseMock.Setup(x => x.AutenticarUsuarioAsync(It.IsAny<LoginRequestDto>())).ReturnsAsync(user);
            using var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/users/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Users")]
        public async Task Login_CredenciaisInvalidas_DeveRetornarUnauthorized()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "ana@teste.com", Password = "errada" };
            _factory.UserUseCaseMock.Reset();
            _factory.UserUseCaseMock.Setup(x => x.AutenticarUsuarioAsync(It.IsAny<LoginRequestDto>())).ReturnsAsync((User?)null);
            using var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/users/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
