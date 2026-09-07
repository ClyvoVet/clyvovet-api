using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Domain.Entities;

namespace ClyvoVet.API.Application.Mappers
{
    public static class UserMapper
    {
        public static User ToUserEntity(this UserRequestDto obj)
        {
            return new User
            {
                Name = obj.Name,
                Email = obj.Email,
                Password = obj.Password,
                Phone = obj.Phone,
                Address = obj.Address
            };
        }
    }
}
