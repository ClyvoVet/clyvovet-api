using System.Diagnostics;
using ClyvoVet.API.Application.Dtos;
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

        public async Task<Medicacao?> ObterUmaMedicacaoAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("MedicacaoUseCase.ObterUmaMedicacaoAsync");
            activity?.SetTag("medicacao.id", id);
            return await _medicacaoRepository.ObterUmAsync(id);
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
    }
}
