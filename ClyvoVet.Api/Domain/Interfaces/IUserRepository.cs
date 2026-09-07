using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> ObterUmAsync(Guid id);
        Task<User?> ObterPorCredenciaisAsync(string email, string password);
        Task<bool> ExisteEmailAsync(string email);
        Task<User> AdicionarAsync(User entity);
    }
}
