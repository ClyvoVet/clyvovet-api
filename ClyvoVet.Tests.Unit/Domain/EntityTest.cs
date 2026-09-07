using System.ComponentModel.DataAnnotations;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.Tests.Unit
{
    public class EntityTest
    {
        [Fact]
        [Trait("Domain", "Users")]
        public void User_NovaInstancia_DeveGerarIdentificador()
        {
            // Arrange
            var user = new User
            {
                Name = "Usuario Teste",
                Email = "usuario.teste@example.com",
                Password = "SenhaTeste123456"
            };

            // Act
            var id = user.Id;

            // Assert
            Assert.NotEqual(Guid.Empty, id);
        }

        [Fact]
        [Trait("Domain", "Users")]
        public void User_EmailInvalido_DeveFalharNaValidacao()
        {
            // Arrange
            var user = new User
            {
                Name = "Usuario Teste",
                Email = "email-invalido",
                Password = "123456"
            };
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(user);

            // Act
            var valido = Validator.TryValidateObject(
                user,
                validationContext,
                validationResults,
                validateAllProperties: true);

            // Assert
            Assert.False(valido);
            Assert.Contains(validationResults, result =>
                result.MemberNames.Contains(nameof(User.Email)));
        }

        [Fact]
        [Trait("Domain", "Pets")]
        public void Pet_NovaInstancia_DeveGerarIdEDataDeCriacao()
        {
            // Arrange
            var inicio = DateTime.UtcNow;

            // Act
            var pet = new Pet();

            // Assert
            Assert.NotEqual(Guid.Empty, pet.Id);
            Assert.True(pet.CreatedAt >= inicio);
        }
    }
}
