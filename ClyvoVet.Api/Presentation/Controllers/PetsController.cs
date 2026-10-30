using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.Helpers;
using ClyvoVet.API.Application.Interfaces;
using ClyvoVet.API.Application.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ClyvoVet.API.Presentation.Controllers
{
    [Authorize(Roles = "tutor")]
    [ApiController]
    [Route("api/pets")]
    public class PetsController : ControllerBase
    {
        private readonly IPetUseCase _petUseCase;

        public PetsController(IPetUseCase petUseCase)
        {
            _petUseCase = petUseCase;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Cadastrar pet")]
        public async Task<IActionResult> Post(PetRequestDto model)
        {
            var pet = await _petUseCase.AdicionarPetAsync(model);
            return CreatedAtAction(nameof(Get), new { id = pet.IdPet }, pet.ToResponseDto(HateoasFactory.ItemLinks("pets", pet.IdPet)));
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Listar pets com paginação, ordenação, filtros e HATEOAS")]
        [SwaggerResponse(200, "Pets retornados", typeof(PageResponseDto<PetResponseDto>))]
        [SwaggerResponse(204, "Nenhum pet encontrado")]
        public async Task<IActionResult> Get([FromQuery] PetQueryDto query)
        {
            var resultado = await _petUseCase.ObterPetsPaginadosAsync(query);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Obter pet por ID com HATEOAS")]
        public async Task<IActionResult> Get(int id)
        {
            var resultado = await _petUseCase.ObterPetResponseAsync(id);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpGet("tutor/{idTutor:int}")]
        [SwaggerOperation(Summary = "Listar pets por tutor")]
        public async Task<IActionResult> GetByTutor(int idTutor)
        {
            var pets = await _petUseCase.ObterPetsPorTutorAsync(idTutor);
            return Ok(pets.Select(x => x.ToResponseDto(HateoasFactory.ItemLinks("pets", x.IdPet))));
        }

        [HttpGet("especie/{especie}")]
        [SwaggerOperation(Summary = "Listar pets por espécie")]
        public async Task<IActionResult> GetBySpecies(string especie)
        {
            var pets = await _petUseCase.ObterPetsPorEspecieAsync(especie);
            return Ok(pets.Select(x => x.ToResponseDto(HateoasFactory.ItemLinks("pets", x.IdPet))));
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Atualizar pet")]
        public async Task<IActionResult> Put(int id, PetRequestDto model)
        {
            var pet = await _petUseCase.EditarPetAsync(id, model);
            return pet is null ? NotFound() : Ok(pet.ToResponseDto(HateoasFactory.ItemLinks("pets", id)));
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Excluir pet")]
        public async Task<IActionResult> Delete(int id)
        {
            var pet = await _petUseCase.DeletarPetAsync(id);
            return pet is null ? NotFound() : Ok(pet.ToResponseDto());
        }
    }
}
