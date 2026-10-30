using ClyvoVet.API.Domain.Entities;
using ClyvoVet.API.Domain.Models;

namespace ClyvoVet.API.Domain.Interfaces
{
    public interface ITokenService
    {
        TokenResult GerarToken(Tutor tutor);
    }
}
