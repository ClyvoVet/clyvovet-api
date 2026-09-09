using System.ComponentModel.DataAnnotations;

namespace ClyvoVet.API.Application.Dtos
{
    public class TutorRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "O ID do tutor deve ser maior que zero.")]
        public int IdTutor { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2)]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Telefone { get; set; }

        [Required]
        [StringLength(14)]
        public string Cpf { get; set; } = string.Empty;

        [Required]
        [StringLength(100, MinimumLength = 6)]
        public string Senha { get; set; } = string.Empty;
    }
}
