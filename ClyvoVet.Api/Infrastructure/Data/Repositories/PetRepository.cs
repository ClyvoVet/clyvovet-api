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

        public PetRepository(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public async Task<IEnumerable<Pet>> ObterTodosAsync()
        {
            using var activity = ActivitySource.StartActivity("PetRepository.ObterTodosAsync");
            return await _applicationContext.Pets.AsNoTracking().ToListAsync();
        }

        public async Task<Pet?> ObterUmAsync(Guid id)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.ObterUmAsync");
            activity?.SetTag("pet.id", id);
            return await _applicationContext.Pets.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IEnumerable<Pet>> ObterPorTutorAsync(Guid ownerId)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.ObterPorTutorAsync");
            activity?.SetTag("owner.id", ownerId);
            return await _applicationContext.Pets.AsNoTracking().Where(x => x.OwnerId == ownerId).ToListAsync();
        }

        public async Task<IEnumerable<Pet>> ObterPorEspecieAsync(string species)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.ObterPorEspecieAsync");
            activity?.SetTag("pet.species", species);
            return await _applicationContext.Pets.AsNoTracking().Where(x => x.Species.ToLower() == species.ToLower()).ToListAsync();
        }

        public async Task<Pet> AdicionarAsync(Pet entity)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.AdicionarAsync");
            _applicationContext.Pets.Add(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Pet?> EditarAsync(Guid id, Pet entity)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.EditarAsync");
            if (id != entity.Id)
                return null;

            _applicationContext.Pets.Update(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Pet?> DeletarAsync(Guid id)
        {
            using var activity = ActivitySource.StartActivity("PetRepository.DeletarAsync");
            var entity = await _applicationContext.Pets.FirstOrDefaultAsync(x => x.Id == id);
            if (entity is null)
                return null;

            _applicationContext.Pets.Remove(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }
    }
}
