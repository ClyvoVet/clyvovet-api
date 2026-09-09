using System.Diagnostics;
using ClyvoVet.API.Application.Dtos;
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

        public TutorUseCase(ITutorRepository tutorRepository) => _tutorRepository = tutorRepository;

        public async Task<IEnumerable<Tutor>> ObterTodosTutoresAsync()
        {
            using var activity = ActivitySource.StartActivity("TutorUseCase.ObterTodosTutoresAsync");
            return await _tutorRepository.ObterTodosAsync();
        }

        public async Task<Tutor?> ObterUmTutorAsync(int id)
        {
            using var activity = ActivitySource.StartActivity("TutorUseCase.ObterUmTutorAsync");
            activity?.SetTag("tutor.id", id);
            return await _tutorRepository.ObterUmAsync(id);
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
    }
}
