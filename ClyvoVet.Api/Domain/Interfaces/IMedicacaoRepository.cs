using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Domain.Interfaces
{
    public interface IMedicacaoRepository
    {
        Task<IEnumerable<Medicacao>> ObterTodosAsync();
        Task<Medicacao?> ObterUmAsync(int id);
        Task<IEnumerable<Medicacao>> ObterPorPetAsync(int idPet);
        Task<Medicacao> AdicionarAsync(Medicacao entity);
        Task<Medicacao?> EditarAsync(int id, Medicacao entity);
        Task<Medicacao?> DeletarAsync(int id);
    }
}
