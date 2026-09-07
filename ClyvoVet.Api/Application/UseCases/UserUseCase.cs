using System.Diagnostics;
using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.Interfaces;
using ClyvoVet.API.Application.Mappers;
using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Interfaces;

namespace ClyvoVet.API.Application.UseCases
{
    public class UserUseCase : IUserUseCase
    {
        private static readonly ActivitySource ActivitySource = new("ClyvoVet.Application");
        private readonly IUserRepository _userRepository;

        public UserUseCase(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User?> ObterUmUsuarioAsync(Guid id)
        {
            using var activity = ActivitySource.StartActivity("UserUseCase.ObterUmUsuarioAsync");
            activity?.SetTag("user.id", id);
            return await _userRepository.ObterUmAsync(id);
        }

        public async Task<User?> RegistrarUsuarioAsync(UserRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("UserUseCase.RegistrarUsuarioAsync");

            if (await _userRepository.ExisteEmailAsync(model.Email))
                return null;

            var entity = model.ToUserEntity();
            return await _userRepository.AdicionarAsync(entity);
        }

        public async Task<User?> AutenticarUsuarioAsync(LoginRequestDto model)
        {
            using var activity = ActivitySource.StartActivity("UserUseCase.AutenticarUsuarioAsync");
            return await _userRepository.ObterPorCredenciaisAsync(model.Email, model.Password);
        }
    }
}
