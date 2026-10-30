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
    public class TutorUseCase : ITutorUseCase
    {
        private static readonly ActivitySource ActivitySource = new("ClyvoVet.Application");
        private readonly ITutorRepository _tutorRepository;
        private readonly ITokenService? _tokenService;

        public TutorUseCase(ITutorRepository tutorRepository)
        {
            _tutorRepository = tutorRepository;
        }

        public TutorUseCase(ITutorRepository tutorRepository, ITokenService tokenService)
        {
            _tutorRepository = tutorRepository;
            _tokenService = tokenService;
        }

        public async Task<IEnumerable<Tutor>> ObterTodosTutoresAsync()
        {
            using var activity = ActivitySource.StartActivity("TutorUseCase.ObterTodosTutoresAsync");
            return await _tutorRepository.ObterTodosAsync();
        }

        public async Task<OperationResult<PageResponseDto<TutorResponseDto>>> ObterTutoresPaginadosAsync(TutorQueryDto query)
        {
            using var activity = ActivitySource.StartActivity("TutorUseCase.ObterTutoresPaginadosAsync");
            var result = await _tutorRepository.ObterPaginadoAsync(
                query.Deslocamento,
                query.RegistroRetornado,
                query.Nome,
                query.Email,
                query.OrdenarPor ?? "idTutor",
                query.Descendente);

            if (!result.data.Any())
                return OperationResult<PageResponseDto<TutorResponseDto>>.Failure("Não há tutores para os filtros informados.", (int)HttpStatusCode.NoContent);

            var response = new PageResponseDto<TutorResponseDto>
            {
                Items = result.data.Select(x => x.ToResponseDto(HateoasFactory.ItemLinks("tutors", x.IdTutor))),
                Deslocamento = query.Deslocamento,
                RegistroRetornado = query.RegistroRetornado,
                TotalRegistros = result.rowsCount,
                Links = HateoasFactory.PageLinks("tutors", query.Deslocamento, query.RegistroRetornado, result.rowsCount, BuildQuery(query))
            };

            return OperationResult<PageResponseDto<TutorResponseDto>>.Success(response);
        }

        public async Task<Tutor?> ObterUmTutorAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("TutorUseCase.ObterUmTutorAsync");
            activity?.SetTag("tutor.id", id);
            return await _tutorRepository.ObterUmAsync(id);
        }

        public async Task<OperationResult<TutorResponseDto?>> ObterTutorResponseAsync(int id)
        {
            var tutor = await ObterUmTutorAsync(id);
            return tutor is null
                ? OperationResult<TutorResponseDto?>.Failure("Tutor não encontrado.", (int)HttpStatusCode.NotFound)
                : OperationResult<TutorResponseDto?>.Success(tutor.ToResponseDto(HateoasFactory.ItemLinks("tutors", id)));
        }

        public async Task<Tutor?> AdicionarTutorAsync(TutorRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("TutorUseCase.AdicionarTutorAsync");
            if (await _tutorRepository.ExisteEmailAsync(model.Email))
                return null;
            return await _tutorRepository.AdicionarAsync(model.ToTutorEntity());
        }

        public async Task<Tutor?> AutenticarTutorAsync(LoginRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("TutorUseCase.AutenticarTutorAsync");
            return await _tutorRepository.ObterPorCredenciaisAsync(model.Email, model.Senha);
        }

        public async Task<OperationResult<AuthResponseDto?>> AutenticarTutorComJwtAsync(LoginRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("TutorUseCase.AutenticarTutorComJwtAsync");
            var tutor = await _tutorRepository.ObterPorCredenciaisAsync(model.Email, model.Senha);
            if (tutor is null)
                return OperationResult<AuthResponseDto?>.Failure("E-mail ou senha inválidos.", (int)HttpStatusCode.Unauthorized);

            if (_tokenService is null)
                throw new InvalidOperationException("Serviço de token JWT não configurado.");

            var token = _tokenService.GerarToken(tutor);
            return OperationResult<AuthResponseDto?>.Success(new AuthResponseDto
            {
                Email = tutor.Email,
                Jwt = token.Token,
                ExpiresAtUtc = token.ExpiresAtUtc
            });
        }

        public async Task<Tutor?> EditarTutorAsync(int id, TutorRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("TutorUseCase.EditarTutorAsync");
            activity?.SetTag("tutor.id", id);
            var entity = await _tutorRepository.ObterUmAsync(id);
            if (entity is null) return null;
            model.MapToExisting(entity);
            return await _tutorRepository.EditarAsync(id, entity);
        }

        public async Task<Tutor?> DeletarTutorAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("TutorUseCase.DeletarTutorAsync");
            activity?.SetTag("tutor.id", id);
            return await _tutorRepository.DeletarAsync(id);
        }

        private static string BuildQuery(TutorQueryDto query)
        {
            var parts = new List<string>
            {
                $"ordenarPor={Uri.EscapeDataString(query.OrdenarPor ?? "idTutor")}",
                $"direcao={query.Direcao}"
            };
            if (!string.IsNullOrWhiteSpace(query.Nome)) parts.Add($"nome={Uri.EscapeDataString(query.Nome)}");
            if (!string.IsNullOrWhiteSpace(query.Email)) parts.Add($"email={Uri.EscapeDataString(query.Email)}");
            return string.Join("&", parts);
        }
    }
}
