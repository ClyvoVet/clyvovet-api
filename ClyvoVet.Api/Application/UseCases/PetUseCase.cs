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
    public class PetUseCase : IPetUseCase
    {
        private static readonly ActivitySource ActivitySource = new("ClyvoVet.Application");
        private readonly IPetRepository _petRepository;

        public PetUseCase(IPetRepository petRepository) => _petRepository = petRepository;

        public async Task<IEnumerable<Pet>> ObterTodosPetsAsync()
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.ObterTodosPetsAsync");
            return await _petRepository.ObterTodosAsync();
        }

        public async Task<OperationResult<PageResponseDto<PetResponseDto>>> ObterPetsPaginadosAsync(PetQueryDto query)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.ObterPetsPaginadosAsync");
            var result = await _petRepository.ObterPaginadoAsync(
                query.Deslocamento, query.RegistroRetornado, query.Nome, query.Especie, query.IdTutor,
                query.OrdenarPor ?? "idPet", query.Descendente);

            if (!result.data.Any())
                return OperationResult<PageResponseDto<PetResponseDto>>.Failure("Não há pets para os filtros informados.", (int)HttpStatusCode.NoContent);

            var response = new PageResponseDto<PetResponseDto>
            {
                Items = result.data.Select(x => x.ToResponseDto(HateoasFactory.ItemLinks("pets", x.IdPet))),
                Deslocamento = query.Deslocamento,
                RegistroRetornado = query.RegistroRetornado,
                TotalRegistros = result.rowsCount,
                Links = HateoasFactory.PageLinks("pets", query.Deslocamento, query.RegistroRetornado, result.rowsCount, BuildQuery(query))
            };
            return OperationResult<PageResponseDto<PetResponseDto>>.Success(response);
        }

        public async Task<Pet?> ObterUmPetAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.ObterUmPetAsync");
            activity?.SetTag("pet.id", id);
            return await _petRepository.ObterUmAsync(id);
        }

        public async Task<OperationResult<PetResponseDto?>> ObterPetResponseAsync(int id)
        {
            var pet = await ObterUmPetAsync(id);
            return pet is null
                ? OperationResult<PetResponseDto?>.Failure("Pet não encontrado.", 404)
                : OperationResult<PetResponseDto?>.Success(pet.ToResponseDto(HateoasFactory.ItemLinks("pets", id)));
        }

        public async Task<IEnumerable<Pet>> ObterPetsPorTutorAsync(int idTutor)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.ObterPetsPorTutorAsync");
            activity?.SetTag("tutor.id", idTutor);
            return await _petRepository.ObterPorTutorAsync(idTutor);
        }

        public async Task<IEnumerable<Pet>> ObterPetsPorEspecieAsync(string especie)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.ObterPetsPorEspecieAsync");
            activity?.SetTag("pet.especie", especie);
            return await _petRepository.ObterPorEspecieAsync(especie);
        }

        public async Task<Pet> AdicionarPetAsync(PetRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.AdicionarPetAsync");
            return await _petRepository.AdicionarAsync(model.ToPetEntity());
        }

        public async Task<Pet?> EditarPetAsync(int id, PetRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.EditarPetAsync");
            activity?.SetTag("pet.id", id);
            var entity = await _petRepository.ObterUmAsync(id);
            if (entity is null) return null;
            model.MapToExisting(entity);
            return await _petRepository.EditarAsync(id, entity);
        }

        public async Task<Pet?> DeletarPetAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("PetUseCase.DeletarPetAsync");
            activity?.SetTag("pet.id", id);
            return await _petRepository.DeletarAsync(id);
        }

        private static string BuildQuery(PetQueryDto query)
        {
            var parts = new List<string> { $"ordenarPor={Uri.EscapeDataString(query.OrdenarPor ?? "idPet")}", $"direcao={query.Direcao}" };
            if (!string.IsNullOrWhiteSpace(query.Nome)) parts.Add($"nome={Uri.EscapeDataString(query.Nome)}");
            if (!string.IsNullOrWhiteSpace(query.Especie)) parts.Add($"especie={Uri.EscapeDataString(query.Especie)}");
            if (query.IdTutor.HasValue) parts.Add($"idTutor={query.IdTutor.Value}");
            return string.Join("&", parts);
        }
    }
}
