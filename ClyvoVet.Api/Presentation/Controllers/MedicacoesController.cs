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
    [Route("api/medicacoes")]
    public class MedicacoesController : ControllerBase
    {
        private readonly IMedicacaoUseCase _medicacaoUseCase;
        private readonly IPetUseCase _petUseCase;

        public MedicacoesController(IMedicacaoUseCase medicacaoUseCase, IPetUseCase petUseCase)
        {
            _medicacaoUseCase = medicacaoUseCase;
            _petUseCase = petUseCase;
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Cadastrar medicação")]
        public async Task<IActionResult> Post(MedicacaoRequestDto model)
        {
            var medicacao = await _medicacaoUseCase.AdicionarMedicacaoAsync(model);
            return CreatedAtAction(nameof(Get), new { id = medicacao.IdMedicacao }, medicacao.ToResponseDto(HateoasFactory.ItemLinks("medicacoes", medicacao.IdMedicacao)));
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Listar medicações com paginação, ordenação, filtros e HATEOAS")]
        [SwaggerResponse(200, "Medicações retornadas", typeof(PageResponseDto<MedicacaoResponseDto>))]
        [SwaggerResponse(204, "Nenhuma medicação encontrada")]
        public async Task<IActionResult> Get([FromQuery] MedicacaoQueryDto query)
        {
            var resultado = await _medicacaoUseCase.ObterMedicacoesPaginadasAsync(query);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Obter medicação por ID com HATEOAS")]
        public async Task<IActionResult> Get(int id)
        {
            var resultado = await _medicacaoUseCase.ObterMedicacaoResponseAsync(id);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpGet("pet/{idPet:int}")]
        [SwaggerOperation(Summary = "Listar medicações por pet")]
        public async Task<IActionResult> GetByPet(int idPet)
        {
            var pet = await _petUseCase.ObterUmPetAsync(idPet);
            if (pet is null) return NotFound("Pet não encontrado.");

            var medicacoes = await _medicacaoUseCase.ObterMedicacoesPorPetAsync(idPet);
            return Ok(medicacoes.Select(x => x.ToResponseDto(HateoasFactory.ItemLinks("medicacoes", x.IdMedicacao))));
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Atualizar medicação")]
        public async Task<IActionResult> Put(int id, MedicacaoRequestDto model)
        {
            var medicacao = await _medicacaoUseCase.EditarMedicacaoAsync(id, model);
            return medicacao is null ? NotFound() : Ok(medicacao.ToResponseDto(HateoasFactory.ItemLinks("medicacoes", id)));
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Excluir medicação")]
        public async Task<IActionResult> Delete(int id)
        {
            var medicacao = await _medicacaoUseCase.DeletarMedicacaoAsync(id);
            return medicacao is null ? NotFound() : Ok(medicacao.ToResponseDto());
        }
    }
}
