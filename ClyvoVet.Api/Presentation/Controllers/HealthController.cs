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
            Verifica se o processo da API esta em execucao.

            Este check e autocontido e nao consulta banco de dados ou servicos externos.
            """)]
        [SwaggerResponse(200, "API em execucao")]
        [SwaggerResponse(503, "API indisponivel")]
        public async Task<IActionResult> Live(CancellationToken cancellationToken)
        {
            var report = await _healthService.CheckHealthAsync(
                registration => registration.Tags.Contains("live"),
                cancellationToken);

            return BuildResponse(report);
        }

        [HttpGet("db")]
        [SwaggerOperation(
            Summary = "Verificar conexao com Oracle",
            Description = """
            Verifica se a API consegue estabelecer conexao com o banco de dados Oracle.

            O check executa somente a verificacao registrada com a tag **db**.
            """)]
        [SwaggerResponse(200, "Banco Oracle disponivel")]
        [SwaggerResponse(503, "Banco Oracle indisponivel")]
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
