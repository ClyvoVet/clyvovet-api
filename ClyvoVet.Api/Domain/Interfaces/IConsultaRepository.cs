using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Domain.Interfaces
{
    public interface IConsultaRepository
    {
        Task<IEnumerable<Consulta>> ObterTodosAsync();
        Task<Consulta?> ObterUmAsync(int id);
        Task<IEnumerable<Consulta>> ObterPorPetAsync(int idPet);
        Task<Consulta> AdicionarAsync(Consulta entity);
        Task<Consulta?> EditarAsync(int id, Consulta entity);
        Task<Consulta?> DeletarAsync(int id);
    }
}
