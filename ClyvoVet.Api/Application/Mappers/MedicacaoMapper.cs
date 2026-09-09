using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Mappers
{
    public static class MedicacaoMapper
    {
        public static Medicacao ToMedicacaoEntity(this MedicacaoRequestDto obj)
        {
            return new Medicacao
            {
                IdMedicacao = obj.IdMedicacao,
                IdPet = obj.IdPet,
                Nome = obj.Nome,
                Dose = obj.Dose,
                Frequencia = obj.Frequencia,
                DataInicio = obj.DataInicio,
                DataFim = obj.DataFim
            };
        }

        public static void MapToExisting(this MedicacaoRequestDto obj, Medicacao entity)
        {
            entity.IdPet = obj.IdPet;
            entity.Nome = obj.Nome;
            entity.Dose = obj.Dose;
            entity.Frequencia = obj.Frequencia;
            entity.DataInicio = obj.DataInicio;
            entity.DataFim = obj.DataFim;
        }
    }
}
