using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Domain.Interfaces
{
    public interface ITutorRepository
    {
        Task<IEnumerable<Tutor>> ObterTodosAsync();
        Task<Tutor?> ObterUmAsync(int id);
        Task<Tutor?> ObterPorCredenciaisAsync(string email, string senha);
        Task<bool> ExisteEmailAsync(string email);
        Task<Tutor> AdicionarAsync(Tutor entity);
        Task<Tutor?> EditarAsync(int id, Tutor entity);
        Task<Tutor?> DeletarAsync(int id);
    }
}
