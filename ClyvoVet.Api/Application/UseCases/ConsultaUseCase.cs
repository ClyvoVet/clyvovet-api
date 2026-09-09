using System.Diagnostics;
using ClyvoVet.API.Application.Dtos;
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

        public async Task<Consulta?> ObterUmaConsultaAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("ConsultaUseCase.ObterUmaConsultaAsync");
            activity?.SetTag("consulta.id", id);
            return await _consultaRepository.ObterUmAsync(id);
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
    }
}
