using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ClyvoVet.API.Presentation.Controllers
{
    [Authorize(Roles = "tutor")]
    [ApiController]
    [Route("api/historicos-clinicos")]
    public class HistoricosClinicosController : ControllerBase
    {
        private readonly IHistoricoClinicoUseCase _useCase;

        public HistoricosClinicosController(IHistoricoClinicoUseCase useCase)
        {
            _useCase = useCase;
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Listar históricos clínicos armazenados no MongoDB")]
        public async Task<IActionResult> Get([FromQuery] HistoricoClinicoQueryDto query)
        {
            var resultado = await _useCase.ObterTodosAsync(query);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Obter histórico clínico por ObjectId")]
        public async Task<IActionResult> Get(string id)
        {
            var resultado = await _useCase.ObterUmAsync(id);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Cadastrar histórico clínico no MongoDB")]
        public async Task<IActionResult> Post(HistoricoClinicoRequestDto model)
        {
            var resultado = await _useCase.AdicionarAsync(model);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Atualizar histórico clínico no MongoDB")]
        public async Task<IActionResult> Put(string id, HistoricoClinicoRequestDto model)
        {
            var resultado = await _useCase.EditarAsync(id, model);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpDelete("{id}")]
        [SwaggerOperation(Summary = "Excluir histórico clínico do MongoDB")]
        public async Task<IActionResult> Delete(string id)
        {
            var resultado = await _useCase.DeletarAsync(id);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }
    }
}
