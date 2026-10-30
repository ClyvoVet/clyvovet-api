using System.Net;
using System.Net.Http.Json;
using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;
using Moq;

namespace ClyvoVet.Tests.Integration
{
    [Collection("API Integration")]
    public class TutorControllerTest
    {
        private readonly CustomWebApplicationFactory _factory;
        public TutorControllerTest(CustomWebApplicationFactory factory) => _factory = factory;

        [Fact]
        [Trait("Controller", "Tutores")]
        public async Task Post_TutorValido_DeveRetornarCreated()
        {
            // Arrange
            var request = new TutorRequestDto { IdTutor = 1, Nome = "Ana", Email = "ana@example.com", Cpf = "104.332.181-00", Senha = "123456" };
            _factory.TutorUseCaseMock.Reset();
            _factory.TutorUseCaseMock.Setup(x => x.AdicionarTutorAsync(It.IsAny<TutorRequestDto>())).ReturnsAsync(new Tutor
            {
                IdTutor = 1, Nome = request.Nome, Email = request.Email, Cpf = request.Cpf, Senha = request.Senha
            });
            using var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/tutors", request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Tutores")]
        public async Task Post_EmailExistente_DeveRetornarConflict()
        {
            // Arrange
            var request = new TutorRequestDto { IdTutor = 1, Nome = "Ana", Email = "ana@example.com", Cpf = "104.332.181-00", Senha = "123456" };
            _factory.TutorUseCaseMock.Reset();
            _factory.TutorUseCaseMock.Setup(x => x.AdicionarTutorAsync(It.IsAny<TutorRequestDto>())).ReturnsAsync((Tutor?)null);
            using var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/tutors", request);

            // Assert
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Tutores")]
        public async Task Login_CredenciaisValidas_DeveRetornarOK()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "ana@example.com", Senha = "123456" };
            _factory.TutorUseCaseMock.Reset();
            _factory.TutorUseCaseMock.Setup(x => x.AutenticarTutorComJwtAsync(It.IsAny<LoginRequestDto>()))
                .ReturnsAsync(OperationResult<AuthResponseDto?>.Success(new AuthResponseDto { Email = request.Email, Jwt = "token", ExpiresAtUtc = DateTime.UtcNow.AddHours(1) }));
            using var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/tutors/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Tutores")]
        public async Task Login_CredenciaisInvalidas_DeveRetornarUnauthorized()
        {
            // Arrange
            var request = new LoginRequestDto { Email = "ana@example.com", Senha = "senhaerrada" };
            _factory.TutorUseCaseMock.Reset();
            _factory.TutorUseCaseMock.Setup(x => x.AutenticarTutorComJwtAsync(It.IsAny<LoginRequestDto>()))
                .ReturnsAsync(OperationResult<AuthResponseDto?>.Failure("E-mail ou senha inválidos.", 401));
            using var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("api/tutors/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Tutores")]
        public async Task Get_TutorExistente_DeveRetornarOK()
        {
            // Arrange
            _factory.TutorUseCaseMock.Reset();
            _factory.TutorUseCaseMock.Setup(x => x.ObterTutorResponseAsync(1))
                .ReturnsAsync(OperationResult<TutorResponseDto?>.Success(new TutorResponseDto { IdTutor = 1, Nome = "Ana", Email = "ana@example.com", Cpf = "104.332.181-00" }));
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/tutors/1");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Controller", "Tutores")]
        public async Task Get_TutorInexistente_DeveRetornarNotFound()
        {
            // Arrange
            _factory.TutorUseCaseMock.Reset();
            _factory.TutorUseCaseMock.Setup(x => x.ObterTutorResponseAsync(999))
                .ReturnsAsync(OperationResult<TutorResponseDto?>.Failure("Tutor não encontrado.", 404));
            using var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("api/tutors/999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
