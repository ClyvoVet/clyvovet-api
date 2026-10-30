using System.ComponentModel.DataAnnotations;

namespace ClyvoVet.API.Application.Dtos
{
    public class PaginationQueryDto
    {
        [Range(0, int.MaxValue)]
        public int Deslocamento { get; set; } = 0;

        [Range(1, 100)]
        public int RegistroRetornado { get; set; } = 10;

        public string? OrdenarPor { get; set; }

        [RegularExpression("(?i)^(asc|desc)$", ErrorMessage = "Direcao deve ser 'asc' ou 'desc'.")]
        public string Direcao { get; set; } = "asc";

        public bool Descendente => string.Equals(Direcao, "desc", StringComparison.OrdinalIgnoreCase);
    }

    public class TutorQueryDto : PaginationQueryDto
    {
        public string? Nome { get; set; }
        public string? Email { get; set; }
    }

    public class PetQueryDto : PaginationQueryDto
    {
        public string? Nome { get; set; }
        public string? Especie { get; set; }
        public int? IdTutor { get; set; }
    }

    public class ConsultaQueryDto : PaginationQueryDto
    {
        public int? IdPet { get; set; }
        public string? Veterinario { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }

    public class MedicacaoQueryDto : PaginationQueryDto
    {
        public int? IdPet { get; set; }
        public string? Nome { get; set; }
    }

    public class HistoricoClinicoQueryDto : PaginationQueryDto
    {
        public int? IdPet { get; set; }
        public string? Tipo { get; set; }
    }
}
