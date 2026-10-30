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
    [Route("api/consultas")]
    public class ConsultasController : ControllerBase
    {
        private readonly IConsultaUseCase _consultaUseCase;
        private readonly IPetUseCase _petUseCase;

        public ConsultasController(IConsultaUseCase consultaUseCase, IPetUseCase petUseCase)
        {
            _consultaUseCase = consultaUseCase;
            _petUseCase = petUseCase;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Cadastrar consulta")]
        public async Task<IActionResult> Post(ConsultaRequestDto model)
        {
            var consulta = await _consultaUseCase.AdicionarConsultaAsync(model);
            return CreatedAtAction(nameof(Get), new { id = consulta.IdConsulta }, consulta.ToResponseDto(HateoasFactory.ItemLinks("consultas", consulta.IdConsulta)));
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Listar consultas com paginação, ordenação, filtros e HATEOAS")]
        [SwaggerResponse(200, "Consultas retornadas", typeof(PageResponseDto<ConsultaResponseDto>))]
        [SwaggerResponse(204, "Nenhuma consulta encontrada")]
        public async Task<IActionResult> Get([FromQuery] ConsultaQueryDto query)
        {
            var resultado = await _consultaUseCase.ObterConsultasPaginadasAsync(query);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Obter consulta por ID com HATEOAS")]
        public async Task<IActionResult> Get(int id)
        {
            var resultado = await _consultaUseCase.ObterConsultaResponseAsync(id);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpGet("pet/{idPet:int}")]
        [SwaggerOperation(Summary = "Listar consultas por pet")]
        public async Task<IActionResult> GetByPet(int idPet)
        {
            var pet = await _petUseCase.ObterUmPetAsync(idPet);
            if (pet is null) return NotFound("Pet não encontrado.");

            var consultas = await _consultaUseCase.ObterConsultasPorPetAsync(idPet);
            return Ok(consultas.Select(x => x.ToResponseDto(HateoasFactory.ItemLinks("consultas", x.IdConsulta))));
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Atualizar consulta")]
        public async Task<IActionResult> Put(int id, ConsultaRequestDto model)
        {
            var consulta = await _consultaUseCase.EditarConsultaAsync(id, model);
            return consulta is null ? NotFound() : Ok(consulta.ToResponseDto(HateoasFactory.ItemLinks("consultas", id)));
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Excluir consulta")]
        public async Task<IActionResult> Delete(int id)
        {
            var consulta = await _consultaUseCase.DeletarConsultaAsync(id);
            return consulta is null ? NotFound() : Ok(consulta.ToResponseDto());
        }
    }
}
