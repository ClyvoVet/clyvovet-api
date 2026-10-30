using System.Diagnostics;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.API.Infrastructure.Data.Repositories
{
    public class TutorRepository : ITutorRepository
    {
        private static readonly ActivitySource ActivitySource = new("ClyvoVet.Infrastructure");
        private readonly ApplicationContext _applicationContext;

        public TutorRepository(ApplicationContext applicationContext) => _applicationContext = applicationContext;

        public async Task<IEnumerable<Tutor>> ObterTodosAsync()
        {
            using var activity = ActivitySource.StartActivity("TutorRepository.ObterTodosAsync");
            return await _applicationContext.Tutores.AsNoTracking().ToListAsync();
        }

        public async Task<(IEnumerable<Tutor> data, int rowsCount)> ObterPaginadoAsync(
            int deslocamento, int registroRetornado, string? nome, string? email, string ordenarPor, bool descendente)
        {
            using var activity = ActivitySource.StartActivity("TutorRepository.ObterPaginadoAsync");
            deslocamento = Math.Max(0, deslocamento);
            registroRetornado = registroRetornado <= 0 ? 10 : Math.Min(registroRetornado, 100);

            var query = _applicationContext.Tutores.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(x => x.Nome.ToLower().Contains(nome.ToLower()));
            if (!string.IsNullOrWhiteSpace(email))
                query = query.Where(x => x.Email.ToLower().Contains(email.ToLower()));

            var total = await query.CountAsync();
            query = (ordenarPor.ToLowerInvariant(), descendente) switch
            {
                ("nome", false) => query.OrderBy(x => x.Nome),
                ("nome", true) => query.OrderByDescending(x => x.Nome),
                ("email", false) => query.OrderBy(x => x.Email),
                ("email", true) => query.OrderByDescending(x => x.Email),
                ("idtutor", true) => query.OrderByDescending(x => x.IdTutor),
                _ => query.OrderBy(x => x.IdTutor)
            };

            var data = await query.Skip(deslocamento).Take(registroRetornado).ToListAsync();
            return (data, total);
        }

        public async Task<Tutor?> ObterUmAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("TutorRepository.ObterUmAsync");
            activity?.SetTag("tutor.id", id);
            return await _applicationContext.Tutores.AsNoTracking().FirstOrDefaultAsync(x => x.IdTutor == id);
        }

        public async Task<Tutor?> ObterPorCredenciaisAsync(string email, string senha)
        {
            using var activity = ActivitySource.StartActivity("TutorRepository.ObterPorCredenciaisAsync");
            return await _applicationContext.Tutores.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email && x.Senha == senha);
        }

        public async Task<bool> ExisteEmailAsync(string email)
        {
            using var activity = ActivitySource.StartActivity("TutorRepository.ExisteEmailAsync");
            return await _applicationContext.Tutores.AnyAsync(x => x.Email == email);
        }

        public async Task<Tutor> AdicionarAsync(Tutor entity)
        {
            using var activity = ActivitySource.StartActivity("TutorRepository.AdicionarAsync");
            _applicationContext.Tutores.Add(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Tutor?> EditarAsync(int id, Tutor entity)
        {
            using var activity = ActivitySource.StartActivity("TutorRepository.EditarAsync");
            if (id != entity.IdTutor) return null;
            _applicationContext.Tutores.Update(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Tutor?> DeletarAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("TutorRepository.DeletarAsync");
            var entity = await _applicationContext.Tutores.FirstOrDefaultAsync(x => x.IdTutor == id);
            if (entity is null) return null;
            _applicationContext.Tutores.Remove(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }
    }
}
