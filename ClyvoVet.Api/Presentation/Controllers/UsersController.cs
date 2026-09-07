using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.Interfaces;
using ClyvoVet.API.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Swashbuckle.AspNetCore.Annotations;

namespace ClyvoVet.API.Presentation.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserUseCase _userUseCase;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserUseCase userUseCase, ILogger<UsersController> logger)
        {
            _userUseCase = userUseCase;
            _logger = logger;
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Cadastrar usuário",
            Description = """
            Cadastra um novo usuário na aplicação.

            ### Dados utilizados no cadastro
            * **Name:** nome do usuário.
            * **Email:** e-mail utilizado no cadastro e autenticação.
            * **Password:** senha do usuário, com no mínimo 6 caracteres.
            * **Phone:** telefone do usuário.
            * **Address:** endereço do usuário.

            ### Fluxo de processamento
            1. Valida os dados recebidos pelo DTO.
            2. Verifica se já existe um usuário cadastrado com o mesmo e-mail.
            3. Converte o DTO em entidade de domínio.
            4. Persiste o novo usuário através do Repository.

            """)]
        [SwaggerResponse(statusCode: 201, description: "Usuario cadastrado com sucesso", type: typeof(User))]
        [SwaggerResponse(statusCode: 400, description: "Dados invalidos, e-mail ja cadastrado ou ocorreu um erro durante o cadastro")]
        public async Task<ActionResult<User>> Register(UserRequestDto model)
        {
            try
            {
                var user = await _userUseCase.RegistrarUsuarioAsync(model);
                if (user is null)
                {
                    _logger.LogWarning("Tentativa de cadastro com e-mail já existente: {Email}", model.Email);
                    return BadRequest("Este e-mail já está registado.");
                }

                return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar usuário");
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Autenticar usuário",
            Description = """
            Valida as credenciais informadas e autentica um usuário cadastrado.

            ### Dados de entrada
            * **Email:** e-mail do usuário.
            * **Password:** senha correspondente ao cadastro.

            ### Fluxo de processamento
            1. Recebe e valida as credenciais.
            2. Consulta o usuário através da camada de Application e Repository.
            3. Retorna o usuário quando as credenciais são válidas.

            ### Observação
            * Este endpoint possui Rate Limiting para limitar tentativas consecutivas.
            """)]
        [SwaggerResponse(statusCode: 200, description: "Usuario autenticado com sucesso")]
        [SwaggerResponse(statusCode: 401, description: "E-mail ou senha incorretos")]
        [SwaggerResponse(statusCode: 400, description: "Dados invalidos ou ocorreu um erro durante a autenticacao")]
        [SwaggerResponse(statusCode: 429, description: "Limite de tentativas de autenticacao excedido")]
        [EnableRateLimiting("politica_5_tentativas")]
        public async Task<ActionResult<User>> Login(LoginRequestDto model)
        {
            try
            {
                var user = await _userUseCase.AutenticarUsuarioAsync(model);
                if (user is null)
                {
                    _logger.LogWarning("Falha de autenticação para {Email}", model.Email);
                    return Unauthorized("E-mail ou palavra-passe incorretos.");
                }

                _logger.LogInformation("Usuário {UserId} autenticado com sucesso", user.Id);
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro durante autenticação");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Obter usuário por ID",
            Description = """
            Busca um usuário específico utilizando seu identificador único.

            ### Fluxo de processamento
            1. Recebe o **ID** do usuário pela rota.
            2. Consulta o registro através da camada de Application.
            3. Retorna o usuário quando encontrado.

            """)]
        [SwaggerResponse(statusCode: 200, description: "Usuario encontrado com sucesso")]
        [SwaggerResponse(statusCode: 404, description: "Usuario nao encontrado")]
        [SwaggerResponse(statusCode: 400, description: "Ocorreu um erro ao consultar o usuario")]
        public async Task<IActionResult> Get(Guid id)
        {
            try
            {
                var user = await _userUseCase.ObterUmUsuarioAsync(id);
                if (user is null)
                    return NotFound();

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter usuário {UserId}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}
