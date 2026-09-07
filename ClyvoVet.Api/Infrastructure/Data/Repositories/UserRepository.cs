using System.Diagnostics;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.API.Infrastructure.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private static readonly ActivitySource ActivitySource = new("ClyvoVet.Infrastructure");
        private readonly ApplicationContext _applicationContext;

        public UserRepository(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public async Task<User?> ObterUmAsync(Guid id)
        {
            using var activity = ActivitySource.StartActivity("UserRepository.ObterUmAsync");
            activity?.SetTag("user.id", id);
            return await _applicationContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<User?> ObterPorCredenciaisAsync(string email, string password)
        {
            using var activity = ActivitySource.StartActivity("UserRepository.ObterPorCredenciaisAsync");
            return await _applicationContext.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email && x.Password == password);
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            using var activity = ActivitySource.StartActivity("UserRepository.ExisteEmailAsync");

            // Evita a traducao de AnyAsync para literais TRUE/FALSE em versoes
            // do Oracle que nao possuem BOOLEAN SQL nativo.
            var quantidade = await _applicationContext.Users
                .CountAsync(x => x.Email == email);

            return quantidade > 0;
        }

        public async Task<User> AdicionarAsync(User entity)
        {
            using var activity = ActivitySource.StartActivity("UserRepository.AdicionarAsync");
            _applicationContext.Users.Add(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }
    }
}
