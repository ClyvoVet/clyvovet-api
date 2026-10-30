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
    public class MedicacaoUseCase : IMedicacaoUseCase
    {
        private static readonly ActivitySource ActivitySource = new("ClyvoVet.Application");
        private readonly IMedicacaoRepository _medicacaoRepository;

        public MedicacaoUseCase(IMedicacaoRepository medicacaoRepository) => _medicacaoRepository = medicacaoRepository;

        public async Task<IEnumerable<Medicacao>> ObterTodasMedicacoesAsync()
        {
            using var activity = ActivitySource.StartActivity("MedicacaoUseCase.ObterTodasMedicacoesAsync");
            return await _medicacaoRepository.ObterTodosAsync();
        }

        public async Task<OperationResult<PageResponseDto<MedicacaoResponseDto>>> ObterMedicacoesPaginadasAsync(MedicacaoQueryDto query)
        {
            using var activity = ActivitySource.StartActivity("MedicacaoUseCase.ObterMedicacoesPaginadasAsync");
            var result = await _medicacaoRepository.ObterPaginadoAsync(
                query.Deslocamento, query.RegistroRetornado, query.IdPet, query.Nome,
                query.OrdenarPor ?? "idMedicacao", query.Descendente);

            if (!result.data.Any())
                return OperationResult<PageResponseDto<MedicacaoResponseDto>>.Failure("Não há medicações para os filtros informados.", (int)HttpStatusCode.NoContent);

            var response = new PageResponseDto<MedicacaoResponseDto>
            {
                Items = result.data.Select(x => x.ToResponseDto(HateoasFactory.ItemLinks("medicacoes", x.IdMedicacao))),
                Deslocamento = query.Deslocamento,
                RegistroRetornado = query.RegistroRetornado,
                TotalRegistros = result.rowsCount,
                Links = HateoasFactory.PageLinks("medicacoes", query.Deslocamento, query.RegistroRetornado, result.rowsCount, BuildQuery(query))
            };
            return OperationResult<PageResponseDto<MedicacaoResponseDto>>.Success(response);
        }

        public async Task<Medicacao?> ObterUmaMedicacaoAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("MedicacaoUseCase.ObterUmaMedicacaoAsync");
            activity?.SetTag("medicacao.id", id);
            return await _medicacaoRepository.ObterUmAsync(id);
        }

        public async Task<OperationResult<MedicacaoResponseDto?>> ObterMedicacaoResponseAsync(int id)
        {
            var medicacao = await ObterUmaMedicacaoAsync(id);
            return medicacao is null
                ? OperationResult<MedicacaoResponseDto?>.Failure("Medicação não encontrada.", 404)
                : OperationResult<MedicacaoResponseDto?>.Success(medicacao.ToResponseDto(HateoasFactory.ItemLinks("medicacoes", id)));
        }

        public async Task<IEnumerable<Medicacao>> ObterMedicacoesPorPetAsync(int idPet)
        {
            using var activity = ActivitySource.StartActivity("MedicacaoUseCase.ObterMedicacoesPorPetAsync");
            activity?.SetTag("pet.id", idPet);
            return await _medicacaoRepository.ObterPorPetAsync(idPet);
        }

        public async Task<Medicacao> AdicionarMedicacaoAsync(MedicacaoRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("MedicacaoUseCase.AdicionarMedicacaoAsync");
            return await _medicacaoRepository.AdicionarAsync(model.ToMedicacaoEntity());
        }

        public async Task<Medicacao?> EditarMedicacaoAsync(int id, MedicacaoRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("MedicacaoUseCase.EditarMedicacaoAsync");
            activity?.SetTag("medicacao.id", id);
            var entity = await _medicacaoRepository.ObterUmAsync(id);
            if (entity is null) return null;
            model.MapToExisting(entity);
            return await _medicacaoRepository.EditarAsync(id, entity);
        }

        public async Task<Medicacao?> DeletarMedicacaoAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("MedicacaoUseCase.DeletarMedicacaoAsync");
            activity?.SetTag("medicacao.id", id);
            return await _medicacaoRepository.DeletarAsync(id);
        }

        private static string BuildQuery(MedicacaoQueryDto query)
        {
            var parts = new List<string> { $"ordenarPor={Uri.EscapeDataString(query.OrdenarPor ?? "idMedicacao")}", $"direcao={query.Direcao}" };
            if (query.IdPet.HasValue) parts.Add($"idPet={query.IdPet.Value}");
            if (!string.IsNullOrWhiteSpace(query.Nome)) parts.Add($"nome={Uri.EscapeDataString(query.Nome)}");
            return string.Join("&", parts);
        }
    }
}
