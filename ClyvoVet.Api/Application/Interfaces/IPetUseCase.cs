using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Interfaces
{
    public interface IPetUseCase
    {
        Task<IEnumerable<Pet>> ObterTodosPetsAsync();
        Task<Pet?> ObterUmPetAsync(int id);
        Task<IEnumerable<Pet>> ObterPetsPorTutorAsync(int idTutor);
        Task<IEnumerable<Pet>> ObterPetsPorEspecieAsync(string especie);
        Task<Pet> AdicionarPetAsync(PetRequestDto model);
        Task<Pet?> EditarPetAsync(int id, PetRequestDto model);
        Task<Pet?> DeletarPetAsync(int id);
    }
}
