using System.ComponentModel.DataAnnotations;

namespace ClyvoVet.API.Application.Dtos
{
    public class HistoricoClinicoRequestDto
    {
        [Range(1, int.MaxValue)]
        public int IdPet { get; set; }

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Descricao { get; set; } = string.Empty;

        public DateTime? DataRegistro { get; set; }
        public List<string> Tags { get; set; } = [];
    }

    public class HistoricoClinicoResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public int IdPet { get; set; }
        public DateTime DataRegistro { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public IEnumerable<string> Tags { get; set; } = [];
        public List<LinkDto> Links { get; set; } = [];
    }
}
