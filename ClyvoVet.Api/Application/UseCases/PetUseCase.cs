using System.Diagnostics;
using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.Interfaces;
using ClyvoVet.API.Application.Mappers;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;

namespace ClyvoVet.API.Application.UseCases
{
    public class PetUseCase : IPetUseCase
    {
        private static readonly ActivitySource ActivitySource = new("ClyvoVet.Application");
        private readonly IPetRepository _petRepository;

        public PetUseCase(IPetRepository petRepository)
        {
            _petRepository = petRepository;
        }

        public async Task<IEnumerable<Pet>> ObterTodosPetsAsync()
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.ObterTodosPetsAsync");
            return await _petRepository.ObterTodosAsync();
        }

        public async Task<Pet?> ObterUmPetAsync(Guid id)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.ObterUmPetAsync");
            activity?.SetTag("pet.id", id);
            return await _petRepository.ObterUmAsync(id);
        }

        public async Task<IEnumerable<Pet>> ObterPetsPorTutorAsync(Guid ownerId)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.ObterPetsPorTutorAsync");
            activity?.SetTag("owner.id", ownerId);
            return await _petRepository.ObterPorTutorAsync(ownerId);
        }

        public async Task<IEnumerable<Pet>> ObterPetsPorEspecieAsync(string species)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.ObterPetsPorEspecieAsync");
            activity?.SetTag("pet.species", species);
            return await _petRepository.ObterPorEspecieAsync(species);
        }

        public async Task<Pet> AdicionarPetAsync(PetRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.AdicionarPetAsync");
            var entity = model.ToPetEntity();
            return await _petRepository.AdicionarAsync(entity);
        }

        public async Task<Pet?> EditarPetAsync(Guid id, PetRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.EditarPetAsync");
            activity?.SetTag("pet.id", id);

            var entity = await _petRepository.ObterUmAsync(id);
            if (entity is null)
                return null;

            model.MapToExisting(entity);
            return await _petRepository.EditarAsync(id, entity);
        }

        public async Task<Pet?> DeletarPetAsync(Guid id)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.DeletarPetAsync");
            activity?.SetTag("pet.id", id);
            return await _petRepository.DeletarAsync(id);
        }
    }
}
