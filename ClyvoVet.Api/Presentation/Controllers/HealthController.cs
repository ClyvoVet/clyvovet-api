using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Swashbuckle.AspNetCore.Annotations;

namespace ClyvoVet.API.Presentation.Controllers
{
    [Route("api/health2")]
    [ApiController]
    [AllowAnonymous]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthService;

        public HealthController(HealthCheckService healthService)
        {
            _healthService = healthService;
        }

        [HttpGet("live")]
        [SwaggerOperation(
            Summary = "Verificar liveness da API",
            Description = """
            Verifica se o processo da API está em execução.

            ### Fluxo de processamento
            1. Executa somente os Health Checks registrados com a tag **live**.
            2. Consolida o resultado das verificações executadas.
            3. Retorna o status de saúde da API.

            ### Observação
            * Este check é autocontido e não consulta o banco de dados.
            """)]
        [SwaggerResponse(200, "API em execução")]
        [SwaggerResponse(503, "API indisponível")]
        public async Task<IActionResult> Live(CancellationToken cancellationToken)
        {
            var report = await _healthService.CheckHealthAsync(
                registration => registration.Tags.Contains("live"),
                cancellationToken);

            return BuildResponse(report);
        }

        [HttpGet("db")]
        [SwaggerOperation(
            Summary = "Verificar conexão com Oracle",
            Description = """
            Verifica se a API consegue estabelecer conexão com o banco de dados Oracle.

            ### Fluxo de processamento
            1. Executa somente os Health Checks registrados com a tag **db**.
            2. Verifica a disponibilidade da conexão configurada com o Oracle.
            3. Retorna o resultado consolidado da verificação.

            ### Observação
            * Caso o banco não esteja acessível, o endpoint retorna status **503 Service Unavailable**.
            """)]
        [SwaggerResponse(200, "Banco Oracle disponível")]
        [SwaggerResponse(503, "Banco Oracle indisponível")]
        public async Task<IActionResult> Database(CancellationToken cancellationToken)
        {
            var report = await _healthService.CheckHealthAsync(
                registration => registration.Tags.Contains("db"),
                cancellationToken);

            return BuildResponse(report);
        }

        private IActionResult BuildResponse(HealthReport report)
        {
            var result = new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(entry => new
                {
                    name = entry.Key,
                    status = entry.Value.Status.ToString(),
                    description = entry.Value.Description,
                    error = entry.Value.Exception?.Message,
                    durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2)
                })
            };

            return report.Status == HealthStatus.Healthy
                ? Ok(result)
                : StatusCode(StatusCodes.Status503ServiceUnavailable, result);
        }
    }
}
