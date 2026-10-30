using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Mappers
{
    public static class ResponseMapper
    {
        public static TutorResponseDto ToResponseDto(this Tutor entity, IEnumerable<LinkDto>? links = null)
            => new()
            {
                IdTutor = entity.IdTutor,
                Nome = entity.Nome,
                Email = entity.Email,
                Telefone = entity.Telefone,
                Cpf = entity.Cpf,
                Links = links?.ToList() ?? []
            };

        public static PetResponseDto ToResponseDto(this Pet entity, IEnumerable<LinkDto>? links = null)
            => new()
            {
                IdPet = entity.IdPet,
                IdTutor = entity.IdTutor,
                Nome = entity.Nome,
                Especie = entity.Especie,
                Raca = entity.Raca,
                DataNascimento = entity.DataNascimento,
                PesoKg = entity.PesoKg,
                TutorNome = entity.Tutor?.Nome,
                Links = links?.ToList() ?? []
            };

        public static ConsultaResponseDto ToResponseDto(this Consulta entity, IEnumerable<LinkDto>? links = null)
            => new()
            {
                IdConsulta = entity.IdConsulta,
                IdPet = entity.IdPet,
                DataConsulta = entity.DataConsulta,
                Veterinario = entity.Veterinario,
                Observacoes = entity.Observacoes,
                PetNome = entity.Pet?.Nome,
                Links = links?.ToList() ?? []
            };

        public static MedicacaoResponseDto ToResponseDto(this Medicacao entity, IEnumerable<LinkDto>? links = null)
            => new()
            {
                IdMedicacao = entity.IdMedicacao,
                IdPet = entity.IdPet,
                Nome = entity.Nome,
                Dose = entity.Dose,
                Frequencia = entity.Frequencia,
                DataInicio = entity.DataInicio,
                DataFim = entity.DataFim,
                PetNome = entity.Pet?.Nome,
                Links = links?.ToList() ?? []
            };

        public static HistoricoClinico ToHistoricoEntity(this HistoricoClinicoRequestDto dto)
            => new()
            {
                IdPet = dto.IdPet,
                DataRegistro = dto.DataRegistro ?? DateTime.UtcNow,
                Tipo = dto.Tipo,
                Descricao = dto.Descricao,
                Tags = dto.Tags
            };

        public static HistoricoClinicoResponseDto ToResponseDto(this HistoricoClinico entity, IEnumerable<LinkDto>? links = null)
            => new()
            {
                Id = entity.Id.ToString(),
                IdPet = entity.IdPet,
                DataRegistro = entity.DataRegistro,
                Tipo = entity.Tipo,
                Descricao = entity.Descricao,
                Tags = entity.Tags,
                Links = links?.ToList() ?? []
            };
    }
}
