using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Interfaces
{
    public interface IMedicacaoUseCase
    {
        Task<IEnumerable<Medicacao>> ObterTodasMedicacoesAsync();
        Task<OperationResult<PageResponseDto<MedicacaoResponseDto>>> ObterMedicacoesPaginadasAsync(MedicacaoQueryDto query);
        Task<Medicacao?> ObterUmaMedicacaoAsync(int id);
        Task<OperationResult<MedicacaoResponseDto?>> ObterMedicacaoResponseAsync(int id);
        Task<IEnumerable<Medicacao>> ObterMedicacoesPorPetAsync(int idPet);
        Task<Medicacao> AdicionarMedicacaoAsync(MedicacaoRequestDto model);
        Task<Medicacao?> EditarMedicacaoAsync(int id, MedicacaoRequestDto model);
        Task<Medicacao?> DeletarMedicacaoAsync(int id);
    }
}
