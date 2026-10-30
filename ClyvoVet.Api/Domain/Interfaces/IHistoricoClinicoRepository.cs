using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Domain.Interfaces
{
    public interface IHistoricoClinicoRepository
    {
        Task<(IEnumerable<HistoricoClinico> data, int rowsCount)> ObterTodosAsync(
            int deslocamento,
            int registroRetornado,
            int? idPet,
            string? tipo,
            string ordenarPor,
            bool descendente);

        Task<HistoricoClinico?> ObterUmAsync(string id);
        Task<HistoricoClinico> AdicionarAsync(HistoricoClinico entity);
        Task<HistoricoClinico?> EditarAsync(string id, HistoricoClinico entity);
        Task<HistoricoClinico?> DeletarAsync(string id);
    }
}
