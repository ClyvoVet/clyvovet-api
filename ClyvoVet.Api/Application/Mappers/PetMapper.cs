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
                Name = obj.Name,
                Species = obj.Species,
                Breed = obj.Breed,
                Weight = obj.Weight,
                Color = obj.Color,
                NextCheckup = obj.NextCheckup,
                OwnerId = obj.OwnerId
            };
        }

        public static void MapToExisting(this PetRequestDto obj, Pet entity)
        {
            entity.Name = obj.Name;
            entity.Species = obj.Species;
            entity.Breed = obj.Breed;
            entity.Weight = obj.Weight;
            entity.Color = obj.Color;
            entity.NextCheckup = obj.NextCheckup;
            entity.OwnerId = obj.OwnerId;
        }
    }
}
