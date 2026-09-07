using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Interfaces
{
    public interface IPetUseCase
    {
        Task<IEnumerable<Pet>> ObterTodosPetsAsync();
        Task<Pet?> ObterUmPetAsync(Guid id);
        Task<IEnumerable<Pet>> ObterPetsPorTutorAsync(Guid ownerId);
        Task<IEnumerable<Pet>> ObterPetsPorEspecieAsync(string species);
        Task<Pet> AdicionarPetAsync(PetRequestDto model);
        Task<Pet?> EditarPetAsync(Guid id, PetRequestDto model);
        Task<Pet?> DeletarPetAsync(Guid id);
    }
}
