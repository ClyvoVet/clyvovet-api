using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Errors;
using ClyvoVet.API.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;

namespace ClyvoVet.API.Infrastructure.Data.Repositories
{
    public class HistoricoClinicoRepository : IHistoricoClinicoRepository
    {
        private readonly MongoApplicationContext _context;

        public HistoricoClinicoRepository(MongoApplicationContext context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<HistoricoClinico> data, int rowsCount)> ObterTodosAsync(
            int deslocamento,
            int registroRetornado,
            int? idPet,
            string? tipo,
            string ordenarPor,
            bool descendente)
        {
            deslocamento = Math.Max(0, deslocamento);
            registroRetornado = registroRetornado <= 0 ? 10 : Math.Min(registroRetornado, 100);

            var query = _context.HistoricosClinicos.AsQueryable();

            if (idPet.HasValue)
                query = query.Where(x => x.IdPet == idPet.Value);

            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(x => x.Tipo == tipo);

            var total = await query.CountAsync();

            query = (ordenarPor.ToLowerInvariant(), descendente) switch
            {
                ("idpet", false) => query.OrderBy(x => x.IdPet),
                ("idpet", true) => query.OrderByDescending(x => x.IdPet),
                ("tipo", false) => query.OrderBy(x => x.Tipo),
                ("tipo", true) => query.OrderByDescending(x => x.Tipo),
                ("dataregistro", false) => query.OrderBy(x => x.DataRegistro),
                _ => query.OrderByDescending(x => x.DataRegistro)
            };

            var data = await query.Skip(deslocamento).Take(registroRetornado).ToListAsync();
            return (data, total);
        }

        public async Task<HistoricoClinico?> ObterUmAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                throw new ArgumentException("Identificador MongoDB inválido.", nameof(id));

            return await _context.HistoricosClinicos
                .FirstOrDefaultAsync(x => x.Id == objectId);
        }

        public async Task<HistoricoClinico> AdicionarAsync(HistoricoClinico entity)
        {
            _context.HistoricosClinicos.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<HistoricoClinico?> EditarAsync(string id, HistoricoClinico entity)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                throw new ArgumentException("Identificador MongoDB inválido.", nameof(id));

            var atual = await _context.HistoricosClinicos.FirstOrDefaultAsync(x => x.Id == objectId);
            if (atual is null)
                throw new NoContentException("Histórico clínico não encontrado.");

            atual.IdPet = entity.IdPet;
            atual.DataRegistro = entity.DataRegistro;
            atual.Tipo = entity.Tipo;
            atual.Descricao = entity.Descricao;
            atual.Tags = entity.Tags;

            await _context.SaveChangesAsync();
            return atual;
        }

        public async Task<HistoricoClinico?> DeletarAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId))
                throw new ArgumentException("Identificador MongoDB inválido.", nameof(id));

            var entity = await _context.HistoricosClinicos.FirstOrDefaultAsync(x => x.Id == objectId);
            if (entity is null)
                throw new NoContentException("Histórico clínico não encontrado.");

            _context.HistoricosClinicos.Remove(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
