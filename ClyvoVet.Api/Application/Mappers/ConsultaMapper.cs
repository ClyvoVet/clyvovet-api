using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Mappers
{
    public static class ConsultaMapper
    {
        public static Consulta ToConsultaEntity(this ConsultaRequestDto obj)
        {
            return new Consulta
            {
                IdConsulta = obj.IdConsulta,
                IdPet = obj.IdPet,
                DataConsulta = obj.DataConsulta,
                Veterinario = obj.Veterinario,
                Observacoes = obj.Observacoes
            };
        }

        public static void MapToExisting(this ConsultaRequestDto obj, Consulta entity)
        {
            entity.IdPet = obj.IdPet;
            entity.DataConsulta = obj.DataConsulta;
            entity.Veterinario = obj.Veterinario;
            entity.Observacoes = obj.Observacoes;
        }
    }
}
