using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Interfaces
{
    public interface IUserUseCase
    {
        Task<User?> ObterUmUsuarioAsync(Guid id);
        Task<User?> RegistrarUsuarioAsync(UserRequestDto model);
        Task<User?> AutenticarUsuarioAsync(LoginRequestDto model);
    }
}
