namespace ClyvoVet.API.Application.Dtos
{
    public class TutorResponseDto
    {
        public int IdTutor { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public string Cpf { get; set; } = string.Empty;
        public List<LinkDto> Links { get; set; } = [];
    }

    public class PetResponseDto
    {
        public int IdPet { get; set; }
        public int IdTutor { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Especie { get; set; } = string.Empty;
        public string? Raca { get; set; }
        public DateTime? DataNascimento { get; set; }
        public decimal? PesoKg { get; set; }
        public string? TutorNome { get; set; }
        public List<LinkDto> Links { get; set; } = [];
    }

    public class ConsultaResponseDto
    {
        public int IdConsulta { get; set; }
        public int IdPet { get; set; }
        public DateTime DataConsulta { get; set; }
        public string? Veterinario { get; set; }
        public string? Observacoes { get; set; }
        public string? PetNome { get; set; }
        public List<LinkDto> Links { get; set; } = [];
    }

    public class MedicacaoResponseDto
    {
        public int IdMedicacao { get; set; }
        public int IdPet { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Dose { get; set; }
        public string? Frequencia { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? PetNome { get; set; }
        public List<LinkDto> Links { get; set; } = [];
    }

    public class AuthResponseDto
    {
        public string Email { get; set; } = string.Empty;
        public string Jwt { get; set; } = string.Empty;
        public DateTime ExpiresAtUtc { get; set; }
    }
}
