using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Domain.Interfaces
{
    public interface IPetRepository
    {
        Task<IEnumerable<Pet>> ObterTodosAsync();
        Task<Pet?> ObterUmAsync(Guid id);
        Task<IEnumerable<Pet>> ObterPorTutorAsync(Guid ownerId);
        Task<IEnumerable<Pet>> ObterPorEspecieAsync(string species);
        Task<Pet> AdicionarAsync(Pet entity);
        Task<Pet?> EditarAsync(Guid id, Pet entity);
        Task<Pet?> DeletarAsync(Guid id);
    }
}
