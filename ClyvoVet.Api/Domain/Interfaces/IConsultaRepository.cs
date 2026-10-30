using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Domain.Interfaces
{
    public interface IConsultaRepository
    {
        Task<IEnumerable<Consulta>> ObterTodosAsync();
        Task<(IEnumerable<Consulta> data, int rowsCount)> ObterPaginadoAsync(
            int deslocamento, int registroRetornado, int? idPet, string? veterinario, DateTime? dataInicio, DateTime? dataFim, string ordenarPor, bool descendente);
        Task<Consulta?> ObterUmAsync(int id);
        Task<IEnumerable<Consulta>> ObterPorPetAsync(int idPet);
        Task<Consulta> AdicionarAsync(Consulta entity);
        Task<Consulta?> EditarAsync(int id, Consulta entity);
        Task<Consulta?> DeletarAsync(int id);
    }
}
