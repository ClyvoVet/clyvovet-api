using System.Net;
using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.Interfaces;
using ClyvoVet.API.Application.Mappers;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;

namespace ClyvoVet.API.Application.UseCases
{
    public class HistoricoClinicoUseCase : IHistoricoClinicoUseCase
    {
        private readonly IHistoricoClinicoRepository _repository;

        public HistoricoClinicoUseCase(IHistoricoClinicoRepository repository)
        {
            _repository = repository;
        }

        public async Task<OperationResult<PageResponseDto<HistoricoClinicoResponseDto>>> ObterTodosAsync(HistoricoClinicoQueryDto query)
        {
            var result = await _repository.ObterTodosAsync(
                query.Deslocamento,
                query.RegistroRetornado,
                query.IdPet,
                query.Tipo,
                query.OrdenarPor ?? "dataRegistro",
                query.Descendente);

            if (!result.data.Any())
                return OperationResult<PageResponseDto<HistoricoClinicoResponseDto>>.Failure(
                    "Não há históricos clínicos para os filtros informados.",
                    (int)HttpStatusCode.NoContent);

            var items = result.data.Select(item => item.ToResponseDto(ItemLinks(item.Id.ToString())));
            var response = new PageResponseDto<HistoricoClinicoResponseDto>
            {
                Items = items,
                Deslocamento = query.Deslocamento,
                RegistroRetornado = query.RegistroRetornado,
                TotalRegistros = result.rowsCount,
                Links = PageLinks(query, result.rowsCount)
            };

            return OperationResult<PageResponseDto<HistoricoClinicoResponseDto>>.Success(response);
        }

        public async Task<OperationResult<HistoricoClinicoResponseDto?>> ObterUmAsync(string id)
        {
            var entity = await _repository.ObterUmAsync(id);
            if (entity is null)
                return OperationResult<HistoricoClinicoResponseDto?>.Failure("Histórico clínico não encontrado.", 404);

            return OperationResult<HistoricoClinicoResponseDto?>.Success(entity.ToResponseDto(ItemLinks(id)));
        }

        public async Task<OperationResult<HistoricoClinicoResponseDto?>> AdicionarAsync(HistoricoClinicoRequestDto model)
        {
            var entity = await _repository.AdicionarAsync(model.ToHistoricoEntity());
            return OperationResult<HistoricoClinicoResponseDto?>.Success(
                entity.ToResponseDto(ItemLinks(entity.Id.ToString())),
                201);
        }

        public async Task<OperationResult<HistoricoClinicoResponseDto?>> EditarAsync(string id, HistoricoClinicoRequestDto model)
        {
            var entity = await _repository.EditarAsync(id, model.ToHistoricoEntity());
            return entity is null
                ? OperationResult<HistoricoClinicoResponseDto?>.Failure("Histórico clínico não encontrado.", 404)
                : OperationResult<HistoricoClinicoResponseDto?>.Success(entity.ToResponseDto(ItemLinks(id)));
        }

        public async Task<OperationResult<HistoricoClinicoResponseDto?>> DeletarAsync(string id)
        {
            var entity = await _repository.DeletarAsync(id);
            return entity is null
                ? OperationResult<HistoricoClinicoResponseDto?>.Failure("Histórico clínico não encontrado.", 404)
                : OperationResult<HistoricoClinicoResponseDto?>.Success(entity.ToResponseDto(ItemLinks(id)));
        }

        private static List<LinkDto> ItemLinks(string id) =>
        [
            new($"/api/historicos-clinicos/{id}", "self", "GET"),
            new($"/api/historicos-clinicos/{id}", "update", "PUT"),
            new($"/api/historicos-clinicos/{id}", "delete", "DELETE")
        ];

        private static List<LinkDto> PageLinks(HistoricoClinicoQueryDto query, int total)
        {
            var links = new List<LinkDto>
            {
                new($"/api/historicos-clinicos?deslocamento={query.Deslocamento}&registroRetornado={query.RegistroRetornado}&ordenarPor={query.OrdenarPor ?? "dataRegistro"}&direcao={query.Direcao}", "self", "GET"),
                new("/api/historicos-clinicos", "create", "POST")
            };

            var next = query.Deslocamento + query.RegistroRetornado;
            if (next < total)
                links.Add(new($"/api/historicos-clinicos?deslocamento={next}&registroRetornado={query.RegistroRetornado}", "next", "GET"));

            var previous = Math.Max(0, query.Deslocamento - query.RegistroRetornado);
            if (query.Deslocamento > 0)
                links.Add(new($"/api/historicos-clinicos?deslocamento={previous}&registroRetornado={query.RegistroRetornado}", "previous", "GET"));

            return links;
        }
    }
}
