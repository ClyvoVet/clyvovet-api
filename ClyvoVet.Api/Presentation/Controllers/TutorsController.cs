using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.Interfaces;
using ClyvoVet.API.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace ClyvoVet.API.Presentation.Controllers
{
    [ApiController]
    [Route("api/tutors")]
    public class TutorsController : ControllerBase
    {
        private readonly ITutorUseCase _tutorUseCase;
        private readonly ILogger<TutorsController> _logger;

        public TutorsController(ITutorUseCase tutorUseCase, ILogger<TutorsController> logger)
        {
            _tutorUseCase = tutorUseCase;
            _logger = logger;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Cadastrar tutor",
            Description = """
            Cadastra um novo tutor na aplicação. O tutor também representa o usuário autenticável do ClyvoVet.

            ### Dados utilizados no cadastro
            * **IdTutor:** identificador numérico do tutor.
            * **Nome:** nome do tutor.
            * **Email:** e-mail utilizado no cadastro e na autenticação.
            * **Telefone:** telefone do tutor, quando informado.
            * **Cpf:** CPF do tutor.
            * **Senha:** senha utilizada na autenticação, com no mínimo 6 caracteres.

            ### Fluxo de processamento
            1. Valida os dados recebidos pelo DTO.
            2. Verifica se já existe um tutor cadastrado com o mesmo e-mail.
            3. Converte o DTO em entidade de domínio.
            4. Persiste o novo tutor através do Repository.
            5. Retorna o tutor criado.

            ### Observações
            * O **IdTutor** é informado na requisição porque o campo `ID_TUTOR` não é gerado automaticamente pelo banco.
            * A senha é utilizada na autenticação, mas não é retornada nas respostas da API.
            """)]
        [SwaggerResponse(201, "Tutor cadastrado com sucesso", typeof(Tutor))]
        [SwaggerResponse(400, "Dados inválidos, e-mail já cadastrado ou erro durante o cadastro")]
        public async Task<ActionResult<Tutor>> Post(TutorRequestDto model)
        {
            try
            {
                var tutor = await _tutorUseCase.AdicionarTutorAsync(model);
                if (tutor is null)
                {
                    _logger.LogWarning("Tentativa de cadastro de tutor com e-mail já existente: {Email}", model.Email);
                    return BadRequest("Este e-mail já está registrado.");
                }

                _logger.LogInformation("Tutor {TutorId} cadastrado", tutor.IdTutor);
                return CreatedAtAction(nameof(Get), new { id = tutor.IdTutor }, tutor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar tutor");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [EnableRateLimiting("politica_5_tentativas")]
        [SwaggerOperation(
            Summary = "Autenticar tutor",
            Description = """
            Valida as credenciais informadas e autentica um tutor cadastrado.

            ### Dados de entrada
            * **Email:** e-mail cadastrado pelo tutor.
            * **Senha:** senha correspondente ao cadastro.

            ### Fluxo de processamento
            1. Recebe e valida as credenciais.
            2. Consulta o tutor através da camada de Application e Repository.
            3. Retorna o tutor quando as credenciais são válidas.

            ### Observações
            * Este endpoint possui Rate Limiting para limitar tentativas consecutivas de autenticação.
            * A senha não é retornada na resposta da API.
            """)]
        [SwaggerResponse(200, "Tutor autenticado com sucesso", typeof(Tutor))]
        [SwaggerResponse(401, "E-mail ou senha incorretos")]
        [SwaggerResponse(400, "Dados inválidos ou erro durante a autenticação")]
        [SwaggerResponse(429, "Limite de tentativas de autenticação excedido")]
        public async Task<ActionResult<Tutor>> Login(LoginRequestDto model)
        {
            try
            {
                var tutor = await _tutorUseCase.AutenticarTutorAsync(model);
                if (tutor is null)
                {
                    _logger.LogWarning("Falha de autenticação de tutor para {Email}", model.Email);
                    return Unauthorized("E-mail ou senha incorretos.");
                }

                _logger.LogInformation("Tutor {TutorId} autenticado com sucesso", tutor.IdTutor);
                return Ok(tutor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante autenticação do tutor");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [EnableRateLimiting("politica_5_tentativas")]
        [SwaggerOperation(
            Summary = "Listar todos os tutores",
            Description = """
            Retorna a lista completa de tutores cadastrados na aplicação.

            ### Fluxo de processamento
            1. Solicita à camada de Application todos os tutores cadastrados.
            2. A camada de Application utiliza o Repository para consultar a persistência.
            3. Retorna a coleção de tutores quando existirem registros.

            ### Observações
            * Este endpoint possui Rate Limiting.
            * Caso não existam tutores cadastrados, o endpoint retorna status **204 No Content**.
            * A senha dos tutores não é exibida na resposta.
            """)]
        [SwaggerResponse(200, "Lista de tutores retornada com sucesso", typeof(IEnumerable<Tutor>))]
        [SwaggerResponse(204, "Não há tutores cadastrados")]
        [SwaggerResponse(400, "Erro ao consultar os tutores")]
        [SwaggerResponse(429, "Limite de requisições excedido")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var resultado = await _tutorUseCase.ObterTodosTutoresAsync();
                if (!resultado.Any()) return NoContent();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar tutores");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obter tutor por ID",
            Description = """
            Busca um tutor específico utilizando seu identificador numérico.

            ### Fluxo de processamento
            1. Recebe o **ID** do tutor pela rota.
            2. Consulta o registro através da camada de Application.
            3. Retorna o tutor quando encontrado.

            ### Observações
            * Caso o tutor não exista, o endpoint retorna status **404 Not Found**.
            * A senha do tutor não é exibida na resposta.
            """)]
        [SwaggerResponse(200, "Tutor encontrado", typeof(Tutor))]
        [SwaggerResponse(404, "Tutor não encontrado")]
        [SwaggerResponse(400, "Erro ao consultar o tutor")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var tutor = await _tutorUseCase.ObterUmTutorAsync(id);
                return tutor is null ? NotFound() : Ok(tutor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter tutor {TutorId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(
            Summary = "Atualizar tutor",
            Description = """
            Atualiza os dados de um tutor existente utilizando o identificador informado na rota.

            ### Fluxo de processamento
            1. Consulta o tutor pelo **ID** informado.
            2. Caso o registro exista, aplica ao tutor os dados recebidos no DTO.
            3. Persiste as alterações através do Repository.
            4. Retorna o tutor atualizado.

            ### Observações
            * O identificador informado na rota é preservado durante a atualização.
            * A senha pode ser atualizada por este endpoint, mas não é retornada na resposta.
            """)]
        [SwaggerResponse(200, "Tutor atualizado com sucesso", typeof(Tutor))]
        [SwaggerResponse(404, "Tutor não encontrado")]
        [SwaggerResponse(400, "Dados inválidos ou erro durante a atualização")]
        public async Task<IActionResult> Put(int id, TutorRequestDto model)
        {
            try
            {
                var tutor = await _tutorUseCase.EditarTutorAsync(id, model);
                return tutor is null ? NotFound() : Ok(tutor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar tutor {TutorId}", id);
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(
            Summary = "Excluir tutor",
            Description = """
            Remove um tutor cadastrado utilizando seu identificador numérico.

            ### Fluxo de processamento
            1. Localiza o tutor pelo **ID** informado.
            2. Caso exista, solicita a exclusão através da camada de Application e Repository.
            3. Retorna o registro excluído quando a operação é concluída.

            ### Observação
            * A exclusão deve respeitar os relacionamentos existentes do tutor com registros da tabela **PET**.
            """)]
        [SwaggerResponse(200, "Tutor excluído com sucesso", typeof(Tutor))]
        [SwaggerResponse(404, "Tutor não encontrado")]
        [SwaggerResponse(400, "Erro ao excluir o tutor")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var tutor = await _tutorUseCase.DeletarTutorAsync(id);
                return tutor is null ? NotFound() : Ok(tutor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir tutor {TutorId}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}
