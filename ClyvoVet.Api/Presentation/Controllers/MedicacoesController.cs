using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.Interfaces;
using ClyvoVet.API.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ClyvoVet.API.Presentation.Controllers
{
    [ApiController]
    [Route("api/medicacoes")]
    public class MedicacoesController : ControllerBase
    {
        private readonly IMedicacaoUseCase _medicacaoUseCase;
        private readonly ILogger<MedicacoesController> _logger;

        public MedicacoesController(IMedicacaoUseCase medicacaoUseCase, ILogger<MedicacoesController> logger)
        {
            _medicacaoUseCase = medicacaoUseCase;
            _logger = logger;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Cadastrar medicação",
            Description = """
            Cadastra uma nova medicação associada a um pet.

            ### Dados utilizados no cadastro
            * **IdMedicacao:** identificador numérico da medicação.
            * **IdPet:** identificador do pet ao qual a medicação está associada.
            * **Nome:** nome da medicação.
            * **Dose:** dose prescrita.
            * **Frequencia:** frequência de utilização.
            * **DataInicio:** data de início do tratamento.
            * **DataFim:** data de término do tratamento.

            ### Fluxo de processamento
            1. Valida os dados recebidos pelo DTO.
            2. Converte o DTO em entidade de domínio.
            3. Persiste a nova medicação através da camada de Application e Repository.
            4. Retorna a medicação criada.

            ### Observações
            * O **IdPet** deve corresponder a um pet existente, pois a medicação possui relacionamento com a tabela **PET**.
            * O **IdMedicacao** é informado na requisição porque o campo **ID_MEDICACAO** não é gerado automaticamente pelo banco.
            """)]
        [SwaggerResponse(201, "Medicação cadastrada com sucesso", typeof(Medicacao))]
        [SwaggerResponse(400, "Dados inválidos ou erro durante o cadastro")]
        public async Task<ActionResult<Medicacao>> Post(MedicacaoRequestDto model)
        {
            try
            {
                var medicacao = await _medicacaoUseCase.AdicionarMedicacaoAsync(model);
                return CreatedAtAction(nameof(Get), new { id = medicacao.IdMedicacao }, medicacao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar medicação");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Listar todas as medicações",
            Description = """
            Retorna a lista completa de medicações cadastradas na aplicação.

            ### Fluxo de processamento
            1. Solicita à camada de Application todas as medicações cadastradas.
            2. A camada de Application utiliza o Repository para consultar a persistência.
            3. Retorna a coleção de medicações quando existirem registros.

            ### Observações
            * Caso não existam medicações cadastradas, o endpoint retorna status **204 No Content**.
            """)]
        [SwaggerResponse(200, "Lista de medicações retornada com sucesso", typeof(IEnumerable<Medicacao>))]
        [SwaggerResponse(204, "Não há medicações cadastradas")]
        [SwaggerResponse(400, "Erro ao consultar as medicações")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var resultado = await _medicacaoUseCase.ObterTodasMedicacoesAsync();
                if (!resultado.Any()) return NoContent();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar medicações");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obter medicação por ID",
            Description = """
            Busca uma medicação específica utilizando seu identificador numérico.

            ### Fluxo de processamento
            1. Recebe o **ID** da medicação pela rota.
            2. Consulta o registro através da camada de Application.
            3. Retorna a medicação quando encontrada.

            ### Observação
            * Caso não exista uma medicação com o identificador informado, o endpoint retorna status **404 Not Found**.
            """)]
        [SwaggerResponse(200, "Medicação encontrada", typeof(Medicacao))]
        [SwaggerResponse(404, "Medicação não encontrada")]
        [SwaggerResponse(400, "Erro ao consultar o registro")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var medicacao = await _medicacaoUseCase.ObterUmaMedicacaoAsync(id);
                return medicacao is null ? NotFound() : Ok(medicacao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter medicação {MedicacaoId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("pet/{idPet:int}")]
        [SwaggerOperation(
            Summary = "Listar medicações por pet",
            Description = """
            Retorna as medicações associadas ao identificador de um pet.

            ### Parâmetro
            * **idPet:** identificador numérico do pet utilizado como filtro da consulta.

            ### Fluxo de processamento
            1. Recebe o **IdPet** pela rota.
            2. Consulta as medicações relacionadas ao pet através da camada de Application.
            3. Retorna a coleção encontrada.

            ### Observação
            * Caso não existam medicações associadas ao pet, o endpoint retorna uma coleção vazia com status **200 OK**.
            """)]
        [SwaggerResponse(200, "Medicações do pet retornadas com sucesso", typeof(IEnumerable<Medicacao>))]
        [SwaggerResponse(400, "Erro ao consultar as medicações do pet")]
        public async Task<IActionResult> GetByPet(int idPet)
        {
            try { return Ok(await _medicacaoUseCase.ObterMedicacoesPorPetAsync(idPet)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar medicações do pet {PetId}", idPet);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(
            Summary = "Atualizar medicação",
            Description = """
            Atualiza os dados de uma medicação existente utilizando o identificador informado na rota.

            ### Fluxo de processamento
            1. Consulta a medicação pelo **ID** informado.
            2. Caso o registro exista, aplica os dados recebidos no DTO.
            3. Persiste as alterações através do Repository.
            4. Retorna a medicação atualizada.

            ### Observações
            * O identificador informado na rota é preservado durante a atualização.
            * O **IdPet** informado deve corresponder a um pet existente.
            """)]
        [SwaggerResponse(200, "Medicação atualizada com sucesso", typeof(Medicacao))]
        [SwaggerResponse(404, "Medicação não encontrada")]
        [SwaggerResponse(400, "Dados inválidos ou erro durante a atualização")]
        public async Task<IActionResult> Put(int id, MedicacaoRequestDto model)
        {
            try
            {
                var medicacao = await _medicacaoUseCase.EditarMedicacaoAsync(id, model);
                return medicacao is null ? NotFound() : Ok(medicacao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar medicação {MedicacaoId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(
            Summary = "Excluir medicação",
            Description = """
            Remove uma medicação cadastrada utilizando seu identificador numérico.

            ### Fluxo de processamento
            1. Localiza a medicação pelo **ID** informado.
            2. Caso exista, solicita a exclusão através da camada de Application e Repository.
            3. Retorna o registro excluído quando a operação é concluída.
            """)]
        [SwaggerResponse(200, "Medicação excluída com sucesso", typeof(Medicacao))]
        [SwaggerResponse(404, "Medicação não encontrada")]
        [SwaggerResponse(400, "Erro ao excluir a medicação")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var medicacao = await _medicacaoUseCase.DeletarMedicacaoAsync(id);
                return medicacao is null ? NotFound() : Ok(medicacao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir medicação {MedicacaoId}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}
