using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Interfaces
{
    public interface IHistoricoClinicoUseCase
    {
        Task<OperationResult<PageResponseDto<HistoricoClinicoResponseDto>>> ObterTodosAsync(HistoricoClinicoQueryDto query);
        Task<OperationResult<HistoricoClinicoResponseDto?>> ObterUmAsync(string id);
        Task<OperationResult<HistoricoClinicoResponseDto?>> AdicionarAsync(HistoricoClinicoRequestDto model);
        Task<OperationResult<HistoricoClinicoResponseDto?>> EditarAsync(string id, HistoricoClinicoRequestDto model);
        Task<OperationResult<HistoricoClinicoResponseDto?>> DeletarAsync(string id);
    }
}
