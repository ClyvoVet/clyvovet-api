using ClyvoVet.API.Application.Dtos;
using ClyvoVet.API.Application.Interfaces;
using ClyvoVet.API.Application.Mappers;
using ClyvoVet.API.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ClyvoVet.API.Presentation.Controllers
{
    [Authorize(Roles = "tutor")]
    [ApiController]
    [Route("api/tutors")]
    public class TutorsController : ControllerBase
    {
        private readonly ITutorUseCase _tutorUseCase;

        public TutorsController(ITutorUseCase tutorUseCase)
        {
            _tutorUseCase = tutorUseCase;
        }

        [AllowAnonymous]
        [HttpPost]
        [SwaggerOperation(Summary = "Cadastrar tutor")]
        [SwaggerResponse(201, "Tutor cadastrado com sucesso", typeof(TutorResponseDto))]
        [SwaggerResponse(409, "E-mail já cadastrado")]
        public async Task<IActionResult> Post(TutorRequestDto model)
        {
            var tutor = await _tutorUseCase.AdicionarTutorAsync(model);
            if (tutor is null) return Conflict("Este e-mail já está registrado.");
            return CreatedAtAction(nameof(Get), new { id = tutor.IdTutor }, tutor.ToResponseDto());
        }

        [AllowAnonymous]
        [HttpPost("login")]
        [SwaggerOperation(Summary = "Autenticar tutor e emitir JWT")]
        [SwaggerResponse(200, "JWT emitido com sucesso", typeof(AuthResponseDto))]
        [SwaggerResponse(401, "Credenciais inválidas")]
        public async Task<IActionResult> Login(LoginRequestDto model)
        {
            var resultado = await _tutorUseCase.AutenticarTutorComJwtAsync(model);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Listar tutores com paginação, ordenação, filtros e HATEOAS")]
        [SwaggerResponse(200, "Tutores retornados", typeof(PageResponseDto<TutorResponseDto>))]
        [SwaggerResponse(204, "Nenhum tutor encontrado")]
        public async Task<IActionResult> Get([FromQuery] TutorQueryDto query)
        {
            var resultado = await _tutorUseCase.ObterTutoresPaginadosAsync(query);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpGet("{id:int}")]
        [SwaggerOperation(Summary = "Obter tutor por ID com HATEOAS")]
        [SwaggerResponse(200, "Tutor encontrado", typeof(TutorResponseDto))]
        [SwaggerResponse(404, "Tutor não encontrado")]
        public async Task<IActionResult> Get(int id)
        {
            var resultado = await _tutorUseCase.ObterTutorResponseAsync(id);
            return resultado.IsSuccess
                ? StatusCode(resultado.StatusCode, resultado.Value)
                : StatusCode(resultado.StatusCode, resultado.Error);
        }

        [HttpPut("{id:int}")]
        [SwaggerOperation(Summary = "Atualizar tutor")]
        public async Task<IActionResult> Put(int id, TutorRequestDto model)
        {
            var tutor = await _tutorUseCase.EditarTutorAsync(id, model);
            return tutor is null ? NotFound() : Ok(tutor.ToResponseDto());
        }

        [HttpDelete("{id:int}")]
        [SwaggerOperation(Summary = "Excluir tutor")]
        public async Task<IActionResult> Delete(int id)
        {
            var tutor = await _tutorUseCase.DeletarTutorAsync(id);
            return tutor is null ? NotFound() : Ok(tutor.ToResponseDto());
        }
    }
}
