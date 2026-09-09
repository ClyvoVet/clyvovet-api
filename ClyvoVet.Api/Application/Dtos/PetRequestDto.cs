using System.ComponentModel.DataAnnotations;

namespace ClyvoVet.API.Application.Dtos
{
    public class PetRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "O ID do pet deve ser maior que zero.")]
        public int IdPet { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O ID do tutor deve ser maior que zero.")]
        public int IdTutor { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Especie { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Raca { get; set; }

        public DateTime? DataNascimento { get; set; }

        [Range(
            typeof(decimal),
            "0",
            "999.99",
            ErrorMessage = "O peso deve estar entre 0 e 999,99 kg.",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true)]
        public decimal? PesoKg { get; set; }
    }
}