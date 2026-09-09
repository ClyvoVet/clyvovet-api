using System.Diagnostics;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.API.Infrastructure.Data.Repositories
{
    public class MedicacaoRepository : IMedicacaoRepository
    {
        private static readonly ActivitySource ActivitySource = new("ClyvoVet.Infrastructure");
        private readonly ApplicationContext _applicationContext;

        public MedicacaoRepository(ApplicationContext applicationContext) => _applicationContext = applicationContext;

        public async Task<IEnumerable<Medicacao>> ObterTodosAsync()
        {
            using var activity = ActivitySource.StartActivity("MedicacaoRepository.ObterTodosAsync");
            return await _applicationContext.Medicacoes.AsNoTracking().ToListAsync();
        }

        public async Task<Medicacao?> ObterUmAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("MedicacaoRepository.ObterUmAsync");
            activity?.SetTag("medicacao.id", id);
            return await _applicationContext.Medicacoes.FirstOrDefaultAsync(x => x.IdMedicacao == id);
        }

        public async Task<IEnumerable<Medicacao>> ObterPorPetAsync(int idPet)
        {
            using var activity = ActivitySource.StartActivity("MedicacaoRepository.ObterPorPetAsync");
            activity?.SetTag("pet.id", idPet);
            return await _applicationContext.Medicacoes.AsNoTracking().Where(x => x.IdPet == idPet).ToListAsync();
        }

        public async Task<Medicacao> AdicionarAsync(Medicacao entity)
        {
            using var activity = ActivitySource.StartActivity("MedicacaoRepository.AdicionarAsync");
            _applicationContext.Medicacoes.Add(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Medicacao?> EditarAsync(int id, Medicacao entity)
        {
            using var activity = ActivitySource.StartActivity("MedicacaoRepository.EditarAsync");
            if (id != entity.IdMedicacao) return null;
            _applicationContext.Medicacoes.Update(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Medicacao?> DeletarAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("MedicacaoRepository.DeletarAsync");
            var entity = await _applicationContext.Medicacoes.FirstOrDefaultAsync(x => x.IdMedicacao == id);
            if (entity is null) return null;
            _applicationContext.Medicacoes.Remove(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }
    }
}
