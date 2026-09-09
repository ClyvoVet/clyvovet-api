using System.ComponentModel.DataAnnotations;

namespace ClyvoVet.API.Application.Dtos
{
    public class ConsultaRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "O ID da consulta deve ser maior que zero.")]
        public int IdConsulta { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "O ID do pet deve ser maior que zero.")]
        public int IdPet { get; set; }

        [Required]
        public DateTime DataConsulta { get; set; }

        [StringLength(100)]
        public string? Veterinario { get; set; }

        [StringLength(300)]
        public string? Observacoes { get; set; }
    }
}
