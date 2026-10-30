using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Domain.Interfaces
{
    public interface IPetRepository
    {
        Task<IEnumerable<Pet>> ObterTodosAsync();
        Task<(IEnumerable<Pet> data, int rowsCount)> ObterPaginadoAsync(
            int deslocamento, int registroRetornado, string? nome, string? especie, int? idTutor, string ordenarPor, bool descendente);
        Task<Pet?> ObterUmAsync(int id);
        Task<IEnumerable<Pet>> ObterPorTutorAsync(int idTutor);
        Task<IEnumerable<Pet>> ObterPorEspecieAsync(string especie);
        Task<Pet> AdicionarAsync(Pet entity);
        Task<Pet?> EditarAsync(int id, Pet entity);
        Task<Pet?> DeletarAsync(int id);
    }
}
