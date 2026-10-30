using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.UseCases;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;
using ClyvoVet.API.Domain.Models;
using MongoDB.Bson;
using Moq;

namespace ClyvoVet.Tests.Unit
{
    public class Sprint4UseCaseTest
    {
        [Fact]
        [Trait("Sprint4", "HATEOAS")]
        public async Task ObterPetsPaginadosAsync_DadosExistentes_DeveRetornarPaginacaoComLinks()
        {
            // Arrange
            var repository = new Mock<IPetRepository>();
            repository.Setup(x => x.ObterPaginadoAsync(0, 10, null, "Cachorro", null, "idPet", false))
                .ReturnsAsync((new[] { new Pet { IdPet = 1, IdTutor = 1, Nome = "Thor", Especie = "Cachorro" } }.AsEnumerable(), 1));
            var useCase = new PetUseCase(repository.Object);
            var query = new PetQueryDto { Especie = "Cachorro" };

            // Act
            var resultado = await useCase.ObterPetsPaginadosAsync(query);

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.NotNull(resultado.Value);
            Assert.Single(resultado.Value.Items);
            Assert.Contains(resultado.Value.Items.First().Links, link => link.Rel == "self");
            Assert.Contains(resultado.Value.Links, link => link.Rel == "create");
        }

        [Fact]
        [Trait("Sprint4", "JWT")]
        public async Task AutenticarTutorComJwtAsync_CredenciaisValidas_DeveRetornarToken()
        {
            // Arrange
            var repository = new Mock<ITutorRepository>();
            var tokenService = new Mock<ITokenService>();
            var dto = new LoginRequestDto { Email = "ana@example.com", Senha = "123456" };
            var tutor = new Tutor { IdTutor = 1, Nome = "Ana", Email = dto.Email, Cpf = "104.332.181-00", Senha = dto.Senha };
            repository.Setup(x => x.ObterPorCredenciaisAsync(dto.Email, dto.Senha)).ReturnsAsync(tutor);
            tokenService.Setup(x => x.GerarToken(tutor)).Returns(new TokenResult { Token = "jwt-teste", ExpiresAtUtc = DateTime.UtcNow.AddHours(1) });
            var useCase = new TutorUseCase(repository.Object, tokenService.Object);

            // Act
            var resultado = await useCase.AutenticarTutorComJwtAsync(dto);

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Equal("jwt-teste", resultado.Value?.Jwt);
        }

        [Fact]
        [Trait("Sprint4", "MongoDB")]
        public async Task ObterTodosAsync_HistoricosExistentes_DeveRetornarPaginaNoSql()
        {
            // Arrange
            var repository = new Mock<IHistoricoClinicoRepository>();
            repository.Setup(x => x.ObterTodosAsync(0, 10, 1, null, "dataRegistro", false))
                .ReturnsAsync((new[]
                {
                    new HistoricoClinico
                    {
                        Id = ObjectId.GenerateNewId(),
                        IdPet = 1,
                        Tipo = "Observacao",
                        Descricao = "Retorno clínico",
                        DataRegistro = DateTime.UtcNow
                    }
                }.AsEnumerable(), 1));
            var useCase = new HistoricoClinicoUseCase(repository.Object);
            var query = new HistoricoClinicoQueryDto { IdPet = 1, Direcao = "asc" };

            // Act
            var resultado = await useCase.ObterTodosAsync(query);

            // Assert
            Assert.True(resultado.IsSuccess);
            Assert.Single(resultado.Value!.Items);
            Assert.Equal(1, resultado.Value.TotalRegistros);
        }
    }
}
