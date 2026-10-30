using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Interfaces
{
    public interface ITutorUseCase
    {
        Task<IEnumerable<Tutor>> ObterTodosTutoresAsync();
        Task<OperationResult<PageResponseDto<TutorResponseDto>>> ObterTutoresPaginadosAsync(TutorQueryDto query);
        Task<Tutor?> ObterUmTutorAsync(int id);
        Task<OperationResult<TutorResponseDto?>> ObterTutorResponseAsync(int id);
        Task<Tutor?> AdicionarTutorAsync(TutorRequestDto model);
        Task<Tutor?> AutenticarTutorAsync(LoginRequestDto model);
        Task<OperationResult<AuthResponseDto?>> AutenticarTutorComJwtAsync(LoginRequestDto model);
        Task<Tutor?> EditarTutorAsync(int id, TutorRequestDto model);
        Task<Tutor?> DeletarTutorAsync(int id);
    }
}
