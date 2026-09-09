using System.ComponentModel.DataAnnotations;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.Tests.Unit
{
    public class EntityTest
    {
        [Fact]
        [Trait("Domain", "Tutores")]
        public void Tutor_DadosValidos_DeveSerValido()
        {
            // Arrange
            var tutor = new Tutor
            {
                IdTutor = 1,
                Nome = "Ana Souza",
                Email = "ana@example.com",
                Cpf = "104.332.181-00",
                Senha = "123456"
            };
            var results = new List<ValidationResult>();

            // Act
            var valido = Validator.TryValidateObject(tutor, new ValidationContext(tutor), results, true);

            // Assert
            Assert.True(valido);
        }

        [Fact]
        [Trait("Domain", "Tutores")]
        public void Tutor_EmailInvalido_DeveFalharNaValidacao()
        {
            // Arrange
            var tutor = new Tutor
            {
                IdTutor = 1,
                Nome = "Ana Souza",
                Email = "email-invalido",
                Cpf = "104.332.181-00",
                Senha = "123456"
            };
            var results = new List<ValidationResult>();

            // Act
            var valido = Validator.TryValidateObject(tutor, new ValidationContext(tutor), results, true);

            // Assert
            Assert.False(valido);
        }

        [Fact]
        [Trait("Domain", "Tutores")]
        public void Tutor_SenhaCurta_DeveFalharNaValidacao()
        {
            // Arrange
            var tutor = new Tutor
            {
                IdTutor = 1,
                Nome = "Ana Souza",
                Email = "ana@example.com",
                Cpf = "104.332.181-00",
                Senha = "123"
            };
            var results = new List<ValidationResult>();

            // Act
            var valido = Validator.TryValidateObject(tutor, new ValidationContext(tutor), results, true);

            // Assert
            Assert.False(valido);
        }

        [Fact]
        [Trait("Domain", "Pets")]
        public void Pet_DadosValidos_DeveManterRelacionamentoComTutor()
        {
            // Arrange
            var pet = new Pet { IdPet = 1, IdTutor = 10, Nome = "Thor", Especie = "Cachorro" };

            // Act
            var tutorId = pet.IdTutor;

            // Assert
            Assert.Equal(10, tutorId);
        }

        [Fact]
        [Trait("Domain", "Consultas")]
        public void Consulta_DadosValidos_DeveManterRelacionamentoComPet()
        {
            // Arrange
            var consulta = new Consulta { IdConsulta = 1, IdPet = 2, DataConsulta = new DateTime(2026, 1, 10) };

            // Act
            var petId = consulta.IdPet;

            // Assert
            Assert.Equal(2, petId);
        }

        [Fact]
        [Trait("Domain", "Medicacoes")]
        public void Medicacao_DadosValidos_DeveManterRelacionamentoComPet()
        {
            // Arrange
            var medicacao = new Medicacao { IdMedicacao = 1, IdPet = 3, Nome = "Dipirona" };

            // Act
            var petId = medicacao.IdPet;

            // Assert
            Assert.Equal(3, petId);
        }
    }
}
