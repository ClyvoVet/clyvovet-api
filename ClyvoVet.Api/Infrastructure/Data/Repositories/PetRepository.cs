using System.Diagnostics;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.API.Infrastructure.Data.Repositories
{
    public class PetRepository : IPetRepository
    {
        private static readonly ActivitySource ActivitySource = new("ClyvoVet.Infrastructure");
        private readonly ApplicationContext _applicationContext;

        public PetRepository(ApplicationContext applicationContext) => _applicationContext = applicationContext;

        public async Task<IEnumerable<Pet>> ObterTodosAsync()
        {
            using var activity = ActivitySource.StartActivity("PetRepository.ObterTodosAsync");
            return await _applicationContext.Pets.AsNoTracking().Include(x => x.Tutor).ToListAsync();
        }

        public async Task<(IEnumerable<Pet> data, int rowsCount)> ObterPaginadoAsync(
            int deslocamento, int registroRetornado, string? nome, string? especie, int? idTutor, string ordenarPor, bool descendente)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.ObterPaginadoAsync");
            deslocamento = Math.Max(0, deslocamento);
            registroRetornado = registroRetornado <= 0 ? 10 : Math.Min(registroRetornado, 100);

            var query = _applicationContext.Pets.AsNoTracking().Include(x => x.Tutor).AsQueryable();
            if (!string.IsNullOrWhiteSpace(nome))
                query = query.Where(x => x.Nome.ToLower().Contains(nome.ToLower()));
            if (!string.IsNullOrWhiteSpace(especie))
                query = query.Where(x => x.Especie.ToLower().Contains(especie.ToLower()));
            if (idTutor.HasValue)
                query = query.Where(x => x.IdTutor == idTutor.Value);

            var total = await query.CountAsync();
            query = (ordenarPor.ToLowerInvariant(), descendente) switch
            {
                ("nome", false) => query.OrderBy(x => x.Nome),
                ("nome", true) => query.OrderByDescending(x => x.Nome),
                ("especie", false) => query.OrderBy(x => x.Especie),
                ("especie", true) => query.OrderByDescending(x => x.Especie),
                ("idpet", true) => query.OrderByDescending(x => x.IdPet),
                _ => query.OrderBy(x => x.IdPet)
            };

            var data = await query.Skip(deslocamento).Take(registroRetornado).ToListAsync();
            return (data, total);
        }

        public async Task<Pet?> ObterUmAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.ObterUmAsync");
            activity?.SetTag("pet.id", id);
            return await _applicationContext.Pets.AsNoTracking().Include(x => x.Tutor).FirstOrDefaultAsync(x => x.IdPet == id);
        }

        public async Task<IEnumerable<Pet>> ObterPorTutorAsync(int idTutor)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.ObterPorTutorAsync");
            activity?.SetTag("tutor.id", idTutor);
            return await _applicationContext.Pets.AsNoTracking().Include(x => x.Tutor).Where(x => x.IdTutor == idTutor).ToListAsync();
        }

        public async Task<IEnumerable<Pet>> ObterPorEspecieAsync(string especie)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.ObterPorEspecieAsync");
            activity?.SetTag("pet.especie", especie);
            return await _applicationContext.Pets.AsNoTracking().Include(x => x.Tutor).Where(x => x.Especie.ToLower() == especie.ToLower()).ToListAsync();
        }

        public async Task<Pet> AdicionarAsync(Pet entity)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.AdicionarAsync");
            _applicationContext.Pets.Add(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Pet?> EditarAsync(int id, Pet entity)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.EditarAsync");
            if (id != entity.IdPet) return null;
            _applicationContext.Pets.Update(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Pet?> DeletarAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.DeletarAsync");
            var entity = await _applicationContext.Pets.FirstOrDefaultAsync(x => x.IdPet == id);
            if (entity is null) return null;
            _applicationContext.Pets.Remove(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }
    }
}
