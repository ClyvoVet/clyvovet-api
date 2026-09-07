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
            * **Name:** nome do pet.
            * **Species:** espécie do pet.
            * **Breed:** raça do pet.
            * **Weight:** peso do pet, com valor maior que zero.
            * **Color:** cor do pet.
            * **NextCheckup:** data prevista para o próximo acompanhamento.
            * **OwnerId:** identificador do tutor associado ao pet.

            ### Fluxo de processamento
            1. Valida os dados recebidos pelo DTO.
            2. Converte o DTO em entidade de domínio.
            3. Persiste o novo pet através da camada de Application e Repository.
            4. Retorna o pet criado.

            """)]
        [SwaggerResponse(statusCode: 201, description: "Pet cadastrado com sucesso", type: typeof(Pet))]
        [SwaggerResponse(statusCode: 400, description: "Dados invalidos ou ocorreu um erro durante o cadastro")]
        public async Task<ActionResult<Pet>> Post(PetRequestDto model)
        {
            try
            {
                var pet = await _petUseCase.AdicionarPetAsync(model);
                _logger.LogInformation("Pet {PetId} criado para o tutor {OwnerId}", pet.Id, pet.OwnerId);
                return CreatedAtAction(nameof(Get), new { id = pet.Id }, pet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar pet");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Listar todos os pets",
            Description = """
            Retorna a lista completa de pets cadastrados na aplicação.

            ### Observações
            * Este endpoint possui Rate Limiting.
            * Os dados são obtidos pela camada de Application, que utiliza o Repository para acesso à persistência.
            """)]
        [SwaggerResponse(statusCode: 200, description: "Lista de pets retornada com sucesso")]
        [SwaggerResponse(statusCode: 204, description: "Nao ha pets cadastrados no sistema")]
        [SwaggerResponse(statusCode: 400, description: "Ocorreu um erro ao consultar os pets")]
        [SwaggerResponse(statusCode: 429, description: "Limite de requisicoes excedido")]
        [EnableRateLimiting("politica_5_tentativas")]
        public async Task<IActionResult> Get()
        {
            try
            {
                _logger.LogInformation("Iniciando consulta de todos os pets");
                var resultado = await _petUseCase.ObterTodosPetsAsync();

                if (!resultado.Any())
                {
                    _logger.LogWarning("Nenhum pet foi encontrado");
                    return NoContent();
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar pets");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Obter pet por ID",
            Description = """
            Busca um pet específico utilizando seu identificador único.

            ### Fluxo de processamento
            1. Recebe o **ID** do pet pela rota.
            2. Consulta o pet através da camada de Application.
            3. Retorna o registro quando encontrado.

            """)]
        [SwaggerResponse(statusCode: 200, description: "Pet encontrado com sucesso")]
        [SwaggerResponse(statusCode: 404, description: "Pet nao encontrado")]
        [SwaggerResponse(statusCode: 400, description: "Ocorreu um erro ao consultar o pet")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var pet = await _petUseCase.ObterUmPetAsync(id);
                if (pet is null)
                {
                    _logger.LogWarning("Pet {PetId} não encontrado", id);
                    return NotFound();
                }

                return Ok(pet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pet {PetId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("owner/{ownerId:guid}")]
        [SwaggerOperation(
            Summary = "Listar pets por tutor",
            Description = """
            Retorna os pets associados ao identificador de um tutor.

            ### Parâmetro
            * **ownerId:** identificador único do tutor utilizado como filtro da consulta.

            ### Observação
            * Caso não existam pets associados ao tutor, o endpoint retorna uma coleção vazia com status **200**.
            """)]
        [SwaggerResponse(statusCode: 200, description: "Pets do tutor retornados com sucesso")]
        [SwaggerResponse(statusCode: 400, description: "Ocorreu um erro ao consultar os pets do tutor")]
        public async Task<IActionResult> GetByOwner(Guid ownerId)
        {
            try
            {
                var pets = await _petUseCase.ObterPetsPorTutorAsync(ownerId);
                return Ok(pets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar pets do tutor {OwnerId}", ownerId);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("species/{species}")]
        [SwaggerOperation(
            Summary = "Listar pets por espécie",
            Description = """
            Retorna os pets filtrados pela espécie informada na rota.

            ### Parâmetro
            * **species:** espécie utilizada para filtrar os pets cadastrados.

            ### Observação
            * Caso nenhum pet corresponda à espécie informada, o endpoint retorna uma coleção vazia com status **200**.
            """)]
        [SwaggerResponse(statusCode: 200, description: "Pets da especie retornados com sucesso")]
        [SwaggerResponse(statusCode: 400, description: "Ocorreu um erro ao consultar os pets por especie")]
        public async Task<IActionResult> GetBySpecies(string species)
        {
            try
            {
                var pets = await _petUseCase.ObterPetsPorEspecieAsync(species);
                return Ok(pets);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar pets da espécie {Species}", species);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Editar pet",
            Description = """
            Atualiza os dados de um pet existente utilizando o identificador informado na rota.

            ### Fluxo de processamento
            1. Consulta o pet pelo **ID** informado.
            2. Caso o registro exista, aplica ao pet os dados recebidos no DTO.
            3. Persiste as alterações através do Repository.
            4. Retorna o pet atualizado.

            """)]
        [SwaggerResponse(statusCode: 200, description: "Pet atualizado com sucesso")]
        [SwaggerResponse(statusCode: 404, description: "Pet nao encontrado")]
        [SwaggerResponse(statusCode: 400, description: "Dados invalidos ou ocorreu um erro durante a atualizacao")]
        public async Task<IActionResult> Put(Guid id, PetRequestDto model)
        {
            try
            {
                var pet = await _petUseCase.EditarPetAsync(id, model);
                if (pet is null)
                    return NotFound();

                return Ok(pet);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao editar pet {PetId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Excluir pet",
            Description = """
            Remove um pet cadastrado utilizando seu identificador único.

            ### Fluxo de processamento
            1. Localiza o pet pelo **ID** informado.
            2. Caso exista, remove o registro através do Repository.
            3. Finaliza a operação sem conteúdo no corpo da resposta.

            """)]
        [SwaggerResponse(statusCode: 204, description: "Pet excluido com sucesso")]
        [SwaggerResponse(statusCode: 404, description: "Pet nao encontrado")]
        [SwaggerResponse(statusCode: 400, description: "Ocorreu um erro durante a exclusao do pet")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var pet = await _petUseCase.DeletarPetAsync(id);
                if (pet is null)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir pet {PetId}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}
