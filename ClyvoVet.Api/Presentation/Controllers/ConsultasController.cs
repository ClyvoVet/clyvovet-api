using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.Interfaces;
using ClyvoVet.API.Application.UseCases;
using ClyvoVet.API.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ClyvoVet.API.Presentation.Controllers
{
    [ApiController]
    [Route("api/consultas")]
    public class ConsultasController : ControllerBase
    {
        private readonly IConsultaUseCase _consultaUseCase;
        private readonly IPetUseCase _petUseCase;
        private readonly ILogger<ConsultasController> _logger;

        public ConsultasController(IConsultaUseCase consultaUseCase, IPetUseCase petUseCase, ILogger<ConsultasController> logger)
        {
            _consultaUseCase = consultaUseCase;
            _petUseCase = petUseCase;
            _logger = logger;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Cadastrar consulta",
            Description = """
            Cadastra uma nova consulta veterinária associada a um pet.

            ### Dados utilizados no cadastro
            * **IdConsulta:** identificador numérico da consulta.
            * **IdPet:** identificador do pet relacionado à consulta.
            * **DataConsulta:** data da consulta.
            * **Veterinario:** nome do veterinário, quando informado.
            * **Observacoes:** observações sobre a consulta, quando informadas.

            ### Fluxo de processamento
            1. Valida os dados recebidos pelo DTO.
            2. Converte o DTO em entidade de domínio.
            3. Persiste a nova consulta através da camada de Application e Repository.
            4. Retorna a consulta criada.

            ### Observações
            * O **IdPet** deve corresponder a um pet existente, pois a consulta possui relacionamento com a tabela **PET**.
            """)]
        [SwaggerResponse(201, "Consulta cadastrada com sucesso", typeof(Consulta))]
        [SwaggerResponse(400, "Dados inválidos ou erro durante o cadastro")]
        public async Task<ActionResult<Consulta>> Post(ConsultaRequestDto model)
        {
            try
            {
                var consulta = await _consultaUseCase.AdicionarConsultaAsync(model);
                return CreatedAtAction(nameof(Get), new { id = consulta.IdConsulta }, consulta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar consulta");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Listar todas as consultas",
            Description = """
            Retorna a lista completa de consultas veterinárias cadastradas na aplicação.

            ### Fluxo de processamento
            1. Solicita à camada de Application todas as consultas cadastradas.
            2. A camada de Application utiliza o Repository para consultar a persistência.
            3. Retorna a coleção de consultas quando existirem registros.

            ### Observações
            * Caso não existam consultas cadastradas, o endpoint retorna status **204 No Content**.
            """)]
        [SwaggerResponse(200, "Lista de consultas retornada com sucesso", typeof(IEnumerable<Consulta>))]
        [SwaggerResponse(204, "Não há consultas cadastradas")]
        [SwaggerResponse(400, "Erro ao consultar as consultas")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var resultado = await _consultaUseCase.ObterTodasConsultasAsync();
                if (!resultado.Any()) return NoContent();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar consultas");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obter consulta por ID",
            Description = """
            Busca uma consulta veterinária específica utilizando seu identificador numérico.

            ### Fluxo de processamento
            1. Recebe o **ID** da consulta pela rota.
            2. Consulta o registro através da camada de Application.
            3. Retorna a consulta quando encontrada.

            ### Observação
            * Caso não exista uma consulta com o identificador informado, o endpoint retorna status **404 Not Found**.
            """)]
        [SwaggerResponse(200, "Consulta encontrada", typeof(Consulta))]
        [SwaggerResponse(404, "Consulta não encontrada")]
        [SwaggerResponse(400, "Erro ao consultar o registro")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var consulta = await _consultaUseCase.ObterUmaConsultaAsync(id);
                return consulta is null ? NotFound() : Ok(consulta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter consulta {ConsultaId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("pet/{idPet}")]
        [SwaggerOperation(
            Summary = "Listar consultas por pet",
            Description = """
            Lista todas as consultas vinculadas a um pet específico.

            ### Parâmetro
            * **idPet:** identificador numérico do pet.

            ### Fluxo de processamento
            1. Recebe o **ID do pet** pela rota.
            2. Verifica se o pet informado existe.
            3. Caso o pet exista, busca todas as consultas vinculadas a ele.
            4. Retorna a coleção encontrada.

            ### Observação
            * Caso o pet informado não exista, o endpoint retorna **404 Not Found**.
            * Caso o pet exista, mas não possua consultas cadastradas, o endpoint retorna **200 OK** com uma lista vazia.
            """)]
        [SwaggerResponse(200,"Consultas do pet retornadas com sucesso")]
        [SwaggerResponse(404,"Pet não encontrado")]
        [SwaggerResponse(400,"Erro ao listar consultas do pet")]
        public async Task<IActionResult> GetByPet(int idPet)
        {
            try
            {
                var pet = await _petUseCase.ObterUmPetAsync(idPet);
                if (pet is null)
                {
                    _logger.LogWarning(
                        "Pet {PetId} não encontrado ao listar consultas",
                        idPet);
                    return NotFound();
                }
                var consultas =
                    await _consultaUseCase.ObterConsultasPorPetAsync(idPet);
                return Ok(consultas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Erro ao listar consultas do pet {PetId}",idPet);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(
            Summary = "Atualizar consulta",
            Description = """
            Atualiza os dados de uma consulta veterinária existente utilizando o identificador informado na rota.

            ### Fluxo de processamento
            1. Consulta a consulta pelo **ID** informado.
            2. Caso o registro exista, aplica os dados recebidos no DTO.
            3. Persiste as alterações através do Repository.
            4. Retorna a consulta atualizada.

            ### Observações
            * O identificador informado na rota é preservado durante a atualização.
            * O **IdPet** informado deve corresponder a um pet existente.
            * Caso não exista uma consulta com o identificador informado, o endpoint retorna status 404 Not Found.
            """)]
        [SwaggerResponse(200, "Consulta atualizada com sucesso", typeof(Consulta))]
        [SwaggerResponse(404, "Consulta não encontrada")]
        [SwaggerResponse(400, "Dados inválidos ou erro durante a atualização")]
        public async Task<IActionResult> Put(int id, ConsultaRequestDto model)
        {
            try
            {
                var consulta = await _consultaUseCase.EditarConsultaAsync(id, model);
                return consulta is null ? NotFound() : Ok(consulta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar consulta {ConsultaId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(
            Summary = "Excluir consulta",
            Description = """
            Remove uma consulta veterinária cadastrada utilizando seu identificador numérico.

            ### Fluxo de processamento
            1. Localiza a consulta pelo **ID** informado.
            2. Caso exista, solicita a exclusão através da camada de Application e Repository.
            3. Retorna o registro excluído quando a operação é concluída.
            """)]
        [SwaggerResponse(200, "Consulta excluída com sucesso", typeof(Consulta))]
        [SwaggerResponse(404, "Consulta não encontrada")]
        [SwaggerResponse(400, "Erro ao excluir a consulta")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var consulta = await _consultaUseCase.DeletarConsultaAsync(id);
                return consulta is null ? NotFound() : Ok(consulta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir consulta {ConsultaId}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}
