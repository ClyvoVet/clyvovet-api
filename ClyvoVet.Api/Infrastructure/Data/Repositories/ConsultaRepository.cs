using System.Diagnostics;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.API.Infrastructure.Data.Repositories
{
    public class ConsultaRepository : IConsultaRepository
    {
        private static readonly ActivitySource ActivitySource = new("ClyvoVet.Infrastructure");
        private readonly ApplicationContext _applicationContext;

        public ConsultaRepository(ApplicationContext applicationContext) => _applicationContext = applicationContext;

        public async Task<IEnumerable<Consulta>> ObterTodosAsync()
        {
            using var activity = ActivitySource.StartActivity("ConsultaRepository.ObterTodosAsync");
            return await _applicationContext.Consultas.AsNoTracking().Include(x => x.Pet).ToListAsync();
        }

        public async Task<(IEnumerable<Consulta> data, int rowsCount)> ObterPaginadoAsync(
            int deslocamento,
            int registroRetornado,
            int? idPet,
            string? veterinario,
            DateTime? dataInicio,
            DateTime? dataFim,
            string ordenarPor,
            bool descendente)
        {
            using var activity = ActivitySource.StartActivity("ConsultaRepository.ObterPaginadoAsync");
            deslocamento = Math.Max(0, deslocamento);
            registroRetornado = registroRetornado <= 0 ? 10 : Math.Min(registroRetornado, 100);

            var query = _applicationContext.Consultas.AsNoTracking().Include(x => x.Pet).AsQueryable();
            if (idPet.HasValue)
                query = query.Where(x => x.IdPet == idPet.Value);
            if (!string.IsNullOrWhiteSpace(veterinario))
                query = query.Where(x => x.Veterinario != null && x.Veterinario.ToLower().Contains(veterinario.ToLower()));
            if (dataInicio.HasValue)
                query = query.Where(x => x.DataConsulta >= dataInicio.Value);
            if (dataFim.HasValue)
                query = query.Where(x => x.DataConsulta <= dataFim.Value);

            var total = await query.CountAsync();
            query = (ordenarPor.ToLowerInvariant(), descendente) switch
            {
                ("veterinario", false) => query.OrderBy(x => x.Veterinario),
                ("veterinario", true) => query.OrderByDescending(x => x.Veterinario),
                ("dataconsulta", false) => query.OrderBy(x => x.DataConsulta),
                ("dataconsulta", true) => query.OrderByDescending(x => x.DataConsulta),
                ("idconsulta", true) => query.OrderByDescending(x => x.IdConsulta),
                _ => query.OrderBy(x => x.IdConsulta)
            };

            var data = await query.Skip(deslocamento).Take(registroRetornado).ToListAsync();
            return (data, total);
        }

        public async Task<Consulta?> ObterUmAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("ConsultaRepository.ObterUmAsync");
            activity?.SetTag("consulta.id", id);
            return await _applicationContext.Consultas.AsNoTracking().Include(x => x.Pet).FirstOrDefaultAsync(x => x.IdConsulta == id);
        }

        public async Task<IEnumerable<Consulta>> ObterPorPetAsync(int idPet)
        {
            using var activity = ActivitySource.StartActivity("ConsultaRepository.ObterPorPetAsync");
            activity?.SetTag("pet.id", idPet);
            return await _applicationContext.Consultas.AsNoTracking().Include(x => x.Pet).Where(x => x.IdPet == idPet).ToListAsync();
        }

        public async Task<Consulta> AdicionarAsync(Consulta entity)
        {
            using var activity = ActivitySource.StartActivity("ConsultaRepository.AdicionarAsync");
            _applicationContext.Consultas.Add(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Consulta?> EditarAsync(int id, Consulta entity)
        {
            using var activity = ActivitySource.StartActivity("ConsultaRepository.EditarAsync");
            if (id != entity.IdConsulta) return null;
            _applicationContext.Consultas.Update(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }

        public async Task<Consulta?> DeletarAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("ConsultaRepository.DeletarAsync");
            var entity = await _applicationContext.Consultas.FirstOrDefaultAsync(x => x.IdConsulta == id);
            if (entity is null) return null;
            _applicationContext.Consultas.Remove(entity);
            await _applicationContext.SaveChangesAsync();
            return entity;
        }
    }
}
