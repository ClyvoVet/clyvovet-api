using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Infrastructure.Data;
using ClyvoVet.API.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.Tests.Unit
{
    public class UserRepositoryTest
    {
        private readonly ApplicationContext _applicationContext;
        private readonly UserRepository _userRepository;

        public UserRepositoryTest()
        {
            var options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseInMemoryDatabase(databaseName: $"UserRepositoryTest-{Guid.NewGuid()}")
                .Options;

            _applicationContext = new ApplicationContext(options);
            _applicationContext.Database.EnsureCreated();
            _userRepository = new UserRepository(_applicationContext);
        }

        [Fact]
        [Trait("Repository", "Users")]
        public async Task ExisteEmailAsync_EmailExistente_DeveRetornarTrue()
        {
            // Arrange
            var user = new User { Name = "Ana", Email = "ana@teste.com", Password = "123456" };
            _applicationContext.Users.Add(user);
            await _applicationContext.SaveChangesAsync();

            // Act
            var resultado = await _userRepository.ExisteEmailAsync(user.Email);

            // Assert
            Assert.True(resultado);
        }

        [Fact]
        [Trait("Repository", "Users")]
        public async Task ExisteEmailAsync_EmailInexistente_DeveRetornarFalse()
        {
            // Arrange
            var email = "naoexiste@teste.com";

            // Act
            var resultado = await _userRepository.ExisteEmailAsync(email);

            // Assert
            Assert.False(resultado);
        }

        [Fact]
        [Trait("Repository", "Users")]
        public async Task ObterPorCredenciaisAsync_CredenciaisValidas_DeveRetornarUsuario()
        {
            // Arrange
            var user = new User { Name = "Ana", Email = "ana@teste.com", Password = "123456" };
            _applicationContext.Users.Add(user);
            await _applicationContext.SaveChangesAsync();

            // Act
            var resultado = await _userRepository.ObterPorCredenciaisAsync(user.Email, user.Password);

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal(user.Id, resultado.Id);
        }
    }
}
