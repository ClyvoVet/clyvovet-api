using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Mappers
{
    public static class TutorMapper
    {
        public static Tutor ToTutorEntity(this TutorRequestDto obj)
        {
            return new Tutor
            {
                IdTutor = obj.IdTutor,
                Nome = obj.Nome,
                Email = obj.Email,
                Telefone = obj.Telefone,
                Cpf = obj.Cpf,
                Senha = obj.Senha
            };
        }

        public static void MapToExisting(this TutorRequestDto obj, Tutor entity)
        {
            entity.Nome = obj.Nome;
            entity.Email = obj.Email;
            entity.Telefone = obj.Telefone;
            entity.Cpf = obj.Cpf;
            entity.Senha = obj.Senha;
        }
    }
}
