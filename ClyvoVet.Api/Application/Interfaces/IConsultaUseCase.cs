using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Interfaces
{
    public interface IConsultaUseCase
    {
        Task<IEnumerable<Consulta>> ObterTodasConsultasAsync();
        Task<Consulta?> ObterUmaConsultaAsync(int id);
        Task<IEnumerable<Consulta>> ObterConsultasPorPetAsync(int idPet);
        Task<Consulta> AdicionarConsultaAsync(ConsultaRequestDto model);
        Task<Consulta?> EditarConsultaAsync(int id, ConsultaRequestDto model);
        Task<Consulta?> DeletarConsultaAsync(int id);
    }
}
