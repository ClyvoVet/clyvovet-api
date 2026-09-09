using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Mappers
{
    public static class PetMapper
    {
        public static Pet ToPetEntity(this PetRequestDto obj)
        {
            return new Pet
            {
                IdPet = obj.IdPet,
                IdTutor = obj.IdTutor,
                Nome = obj.Nome,
                Especie = obj.Especie,
                Raca = obj.Raca,
                DataNascimento = obj.DataNascimento,
                PesoKg = obj.PesoKg
            };
        }

        public static void MapToExisting(this PetRequestDto obj, Pet entity)
        {
            entity.IdTutor = obj.IdTutor;
            entity.Nome = obj.Nome;
            entity.Especie = obj.Especie;
            entity.Raca = obj.Raca;
            entity.DataNascimento = obj.DataNascimento;
            entity.PesoKg = obj.PesoKg;
        }
    }
}
