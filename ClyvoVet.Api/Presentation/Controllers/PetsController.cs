using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.Interfaces;
using ClyvoVet.API.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace ClyvoVet.API.Presentation.Controllers
{
    [ApiController]
    [Route("api/pets")]
    public class PetsController : ControllerBase
    {
        private readonly IPetUseCase _petUseCase;
        private readonly ILogger<PetsController> _logger;

        public PetsController(IPetUseCase petUseCase, ILogger<PetsController> logger)
        {
            _petUseCase = petUseCase;
            _logger = logger;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Cadastrar pet",
            Description = """
            Cadastra um novo pet utilizando os dados informados na requisição.

            ### Dados utilizados no cadastro
            * **IdPet:** identificador numérico do pet.
            * **IdTutor:** identificador do tutor ao qual o pet pertence.
            * **Nome:** nome do pet.
            * **Especie:** espécie do pet.
            * **Raca:** raça do pet, quando informada.
            * **DataNascimento:** data de nascimento do pet, quando informada.
            * **PesoKg:** peso do pet em quilogramas, quando informado.

            ### Fluxo de processamento
            1. Valida os dados recebidos pelo DTO.
            2. Converte o DTO em entidade de domínio.
            3. Persiste o novo pet através da camada de Application e Repository.
            4. Retorna o pet criado.

            ### Observação
            * O **IdTutor** deve corresponder a um tutor existente, pois o pet possui relacionamento com a tabela **TUTOR**.
            * O **IdPet** é informado na requisição porque o campo `ID_PET` não é gerado automaticamente pelo banco.
            """)]
        [SwaggerResponse(201, "Pet cadastrado com sucesso", typeof(Pet))]
        [SwaggerResponse(400, "Dados inválidos ou erro durante o cadastro")]
        public async Task<ActionResult<Pet>> Post(PetRequestDto model)
        {
            try
            {
                var pet = await _petUseCase.AdicionarPetAsync(model);
                _logger.LogInformation("Pet {PetId} criado para o tutor {TutorId}", pet.IdPet, pet.IdTutor);
                return CreatedAtAction(nameof(Get), new { id = pet.IdPet }, pet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar pet");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [EnableRateLimiting("politica_5_tentativas")]
        [SwaggerOperation(
            Summary = "Listar todos os pets",
            Description = """
            Retorna a lista completa de pets cadastrados na aplicação.

            ### Fluxo de processamento
            1. Solicita à camada de Application todos os pets cadastrados.
            2. A camada de Application utiliza o Repository para consultar a persistência.
            3. Retorna a coleção de pets quando existirem registros.

            ### Observações
            * Este endpoint possui Rate Limiting.
            * Caso não existam pets cadastrados, o endpoint retorna status **204 No Content**.
            """)]
        [SwaggerResponse(200, "Lista de pets retornada com sucesso", typeof(IEnumerable<Pet>))]
        [SwaggerResponse(204, "Não há pets cadastrados")]
        [SwaggerResponse(400, "Erro ao consultar os pets")]
        [SwaggerResponse(429, "Limite de requisições excedido")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var resultado = await _petUseCase.ObterTodosPetsAsync();
                if (!resultado.Any()) return NoContent();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar pets");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obter pet por ID",
            Description = """
            Busca um pet específico utilizando seu identificador numérico.

            ### Fluxo de processamento
            1. Recebe o **ID** do pet pela rota.
            2. Consulta o pet através da camada de Application.
            3. Retorna o registro quando encontrado.

            ### Observação
            * Caso não exista um pet com o identificador informado, o endpoint retorna status **404 Not Found**.
            """)]
        [SwaggerResponse(200, "Pet encontrado", typeof(Pet))]
        [SwaggerResponse(404, "Pet não encontrado")]
        [SwaggerResponse(400, "Erro ao consultar o pet")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var pet = await _petUseCase.ObterUmPetAsync(id);
                return pet is null ? NotFound() : Ok(pet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pet {PetId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("tutor/{idTutor:int}")]
        [SwaggerOperation(
            Summary = "Listar pets por tutor",
            Description = """
            Retorna os pets associados ao identificador de um tutor.

            ### Parâmetro
            * **idTutor:** identificador numérico do tutor utilizado como filtro da consulta.

            ### Fluxo de processamento
            1. Recebe o **IdTutor** pela rota.
            2. Consulta os pets relacionados ao tutor através da camada de Application.
            3. Retorna a coleção encontrada.

            ### Observação
            * Caso não existam pets associados ao tutor, o endpoint retorna uma coleção vazia com status **200 OK**.
            """)]
        [SwaggerResponse(200, "Pets do tutor retornados com sucesso", typeof(IEnumerable<Pet>))]
        [SwaggerResponse(400, "Erro ao consultar os pets do tutor")]
        public async Task<IActionResult> GetByTutor(int idTutor)
        {
            try { return Ok(await _petUseCase.ObterPetsPorTutorAsync(idTutor)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar pets do tutor {TutorId}", idTutor);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("especie/{especie}")]
        [SwaggerOperation(
            Summary = "Listar pets por espécie",
            Description = """
            Retorna os pets filtrados pela espécie informada na rota.

            ### Parâmetro
            * **especie:** espécie utilizada para filtrar os pets cadastrados.

            ### Fluxo de processamento
            1. Recebe a espécie pela rota.
            2. Consulta os pets através da camada de Application utilizando a espécie como filtro.
            3. Retorna a coleção encontrada.

            ### Observação
            * Caso nenhum pet corresponda à espécie informada, o endpoint retorna uma coleção vazia com status **200 OK**.
            """)]
        [SwaggerResponse(200, "Pets da espécie retornados com sucesso", typeof(IEnumerable<Pet>))]
        [SwaggerResponse(400, "Erro ao consultar os pets por espécie")]
        public async Task<IActionResult> GetBySpecies(string especie)
        {
            try { return Ok(await _petUseCase.ObterPetsPorEspecieAsync(especie)); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar pets da espécie {Especie}", especie);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(
            Summary = "Atualizar pet",
            Description = """
            Atualiza os dados de um pet existente utilizando o identificador informado na rota.

            ### Fluxo de processamento
            1. Consulta o pet pelo **ID** informado.
            2. Caso o registro exista, aplica ao pet os dados recebidos no DTO.
            3. Persiste as alterações através do Repository.
            4. Retorna o pet atualizado.

            ### Observação
            * O identificador informado na rota é preservado durante a atualização.
            * O **IdTutor** informado deve corresponder a um tutor existente.
            """)]
        [SwaggerResponse(200, "Pet atualizado com sucesso", typeof(Pet))]
        [SwaggerResponse(404, "Pet não encontrado")]
        [SwaggerResponse(400, "Dados inválidos ou erro durante a atualização")]
        public async Task<IActionResult> Put(int id, PetRequestDto model)
        {
            try
            {
                var pet = await _petUseCase.EditarPetAsync(id, model);
                return pet is null ? NotFound() : Ok(pet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar pet {PetId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(
            Summary = "Excluir pet",
            Description = """
            Remove um pet cadastrado utilizando seu identificador numérico.

            ### Fluxo de processamento
            1. Localiza o pet pelo **ID** informado.
            2. Caso exista, solicita a exclusão através da camada de Application e Repository.
            3. Retorna o registro excluído quando a operação é concluída.

            ### Observação
            * A exclusão deve respeitar os relacionamentos existentes do pet com **CONSULTA** e **MEDICACAO** no banco de dados.
            """)]
        [SwaggerResponse(200, "Pet excluído com sucesso", typeof(Pet))]
        [SwaggerResponse(404, "Pet não encontrado")]
        [SwaggerResponse(400, "Erro ao excluir o pet")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var pet = await _petUseCase.DeletarPetAsync(id);
                return pet is null ? NotFound() : Ok(pet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir pet {PetId}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}
