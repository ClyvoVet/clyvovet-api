using System.ComponentModel.DataAnnotations;

namespace ClyvoVet.API.Application.Dtos
{
    public class MedicacaoRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "O ID da medicação deve ser maior que zero.")]
        public int IdMedicacao { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O ID do pet deve ser maior que zero.")]
        public int IdPet { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Dose { get; set; }

        [StringLength(50)]
        public string? Frequencia { get; set; }

        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }
}
