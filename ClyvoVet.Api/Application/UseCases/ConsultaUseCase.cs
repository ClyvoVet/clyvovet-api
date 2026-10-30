using System.Diagnostics;
using System.Net;
using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.Helpers;
using ClyvoVet.API.Application.Interfaces;
using ClyvoVet.API.Application.Mappers;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;

namespace ClyvoVet.API.Application.UseCases
{
    public class ConsultaUseCase : IConsultaUseCase
    {
        private static readonly ActivitySource ActivitySource = new("ClyvoVet.Application");
        private readonly IConsultaRepository _consultaRepository;

        public ConsultaUseCase(IConsultaRepository consultaRepository) => _consultaRepository = consultaRepository;

        public async Task<IEnumerable<Consulta>> ObterTodasConsultasAsync()
        {
            using var activity = ActivitySource.StartActivity("ConsultaUseCase.ObterTodasConsultasAsync");
            return await _consultaRepository.ObterTodosAsync();
        }

        public async Task<OperationResult<PageResponseDto<ConsultaResponseDto>>> ObterConsultasPaginadasAsync(ConsultaQueryDto query)
        {
            using var activity = ActivitySource.StartActivity("ConsultaUseCase.ObterConsultasPaginadasAsync");
            var result = await _consultaRepository.ObterPaginadoAsync(
                query.Deslocamento, query.RegistroRetornado, query.IdPet, query.Veterinario,
                query.DataInicio, query.DataFim, query.OrdenarPor ?? "idConsulta", query.Descendente);

            if (!result.data.Any())
                return OperationResult<PageResponseDto<ConsultaResponseDto>>.Failure("Não há consultas para os filtros informados.", (int)HttpStatusCode.NoContent);

            var response = new PageResponseDto<ConsultaResponseDto>
            {
                Items = result.data.Select(x => x.ToResponseDto(HateoasFactory.ItemLinks("consultas", x.IdConsulta))),
                Deslocamento = query.Deslocamento,
                RegistroRetornado = query.RegistroRetornado,
                TotalRegistros = result.rowsCount,
                Links = HateoasFactory.PageLinks("consultas", query.Deslocamento, query.RegistroRetornado, result.rowsCount, BuildQuery(query))
            };
            return OperationResult<PageResponseDto<ConsultaResponseDto>>.Success(response);
        }

        public async Task<Consulta?> ObterUmaConsultaAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("ConsultaUseCase.ObterUmaConsultaAsync");
            activity?.SetTag("consulta.id", id);
            return await _consultaRepository.ObterUmAsync(id);
        }

        public async Task<OperationResult<ConsultaResponseDto?>> ObterConsultaResponseAsync(int id)
        {
            var consulta = await ObterUmaConsultaAsync(id);
            return consulta is null
                ? OperationResult<ConsultaResponseDto?>.Failure("Consulta não encontrada.", 404)
                : OperationResult<ConsultaResponseDto?>.Success(consulta.ToResponseDto(HateoasFactory.ItemLinks("consultas", id)));
        }

        public async Task<IEnumerable<Consulta>> ObterConsultasPorPetAsync(int idPet)
        {
            using var activity = ActivitySource.StartActivity("ConsultaUseCase.ObterConsultasPorPetAsync");
            activity?.SetTag("pet.id", idPet);
            return await _consultaRepository.ObterPorPetAsync(idPet);
        }

        public async Task<Consulta> AdicionarConsultaAsync(ConsultaRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("ConsultaUseCase.AdicionarConsultaAsync");
            return await _consultaRepository.AdicionarAsync(model.ToConsultaEntity());
        }

        public async Task<Consulta?> EditarConsultaAsync(int id, ConsultaRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("ConsultaUseCase.EditarConsultaAsync");
            activity?.SetTag("consulta.id", id);
            var entity = await _consultaRepository.ObterUmAsync(id);
            if (entity is null) return null;
            model.MapToExisting(entity);
            return await _consultaRepository.EditarAsync(id, entity);
        }

        public async Task<Consulta?> DeletarConsultaAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("ConsultaUseCase.DeletarConsultaAsync");
            activity?.SetTag("consulta.id", id);
            return await _consultaRepository.DeletarAsync(id);
        }

        private static string BuildQuery(ConsultaQueryDto query)
        {
            var parts = new List<string> { $"ordenarPor={Uri.EscapeDataString(query.OrdenarPor ?? "idConsulta")}", $"direcao={query.Direcao}" };
            if (query.IdPet.HasValue) parts.Add($"idPet={query.IdPet.Value}");
            if (!string.IsNullOrWhiteSpace(query.Veterinario)) parts.Add($"veterinario={Uri.EscapeDataString(query.Veterinario)}");
            if (query.DataInicio.HasValue) parts.Add($"dataInicio={query.DataInicio:O}");
            if (query.DataFim.HasValue) parts.Add($"dataFim={query.DataFim:O}");
            return string.Join("&", parts);
        }
    }
}
