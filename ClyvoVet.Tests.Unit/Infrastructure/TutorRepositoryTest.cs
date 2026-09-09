using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Infrastructure.Data;
using ClyvoVet.API.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.Tests.Unit
{
    public class TutorRepositoryTest
    {
        private static (ApplicationContext Context, TutorRepository Repository) CriarContexto()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase($"TutorRepositoryTest-{Guid.NewGuid()}")
                .Options;
            var context = new ApplicationContext(options);
            context.Database.EnsureCreated();
            return (context, new TutorRepository(context));
        }

        [Fact]
        [Trait("Repository", "Tutores")]
        public async Task AdicionarAsync_TutorValido_DevePersistirTutor()
        {
            // Arrange
            var (context, repository) = CriarContexto();
            await using var contextDisposable = context;
            var tutor = new Tutor
            {
                IdTutor = 1,
                Nome = "Ana",
                Email = "ana@example.com",
                Cpf = "104.332.181-00",
                Senha = "123456"
            };

            // Act
            var resultado = await repository.AdicionarAsync(tutor);

            // Assert
            Assert.NotNull(await context.Tutores.FindAsync(resultado.IdTutor));
        }

        [Fact]
        [Trait("Repository", "Tutores")]
        public async Task ExisteEmailAsync_EmailExistente_DeveRetornarTrue()
        {
            // Arrange
            var (context, repository) = CriarContexto();
            await using var contextDisposable = context;
            var tutor = new Tutor
            {
                IdTutor = 1,
                Nome = "Ana",
                Email = "ana@example.com",
                Cpf = "104.332.181-00",
                Senha = "123456"
            };
            context.Tutores.Add(tutor);
            await context.SaveChangesAsync();

            // Act
            var resultado = await repository.ExisteEmailAsync(tutor.Email);

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        [Trait("Repository", "Tutores")]
        public async Task ExisteEmailAsync_EmailInexistente_DeveRetornarFalse()
        {
            // Arrange
            var (context, repository) = CriarContexto();
            await using var contextDisposable = context;

            // Act
            var resultado = await repository.ExisteEmailAsync("naoexiste@example.com");

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        [Trait("Repository", "Tutores")]
        public async Task ObterPorCredenciaisAsync_CredenciaisValidas_DeveRetornarTutor()
        {
            // Arrange
            var (context, repository) = CriarContexto();
            await using var contextDisposable = context;
            var tutor = new Tutor
            {
                IdTutor = 1,
                Nome = "Ana",
                Email = "ana@example.com",
                Cpf = "104.332.181-00",
                Senha = "123456"
            };
            context.Tutores.Add(tutor);
            await context.SaveChangesAsync();

            // Act
            var resultado = await repository.ObterPorCredenciaisAsync(tutor.Email, tutor.Senha);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(tutor.IdTutor, resultado.IdTutor);
        }
    }
}
