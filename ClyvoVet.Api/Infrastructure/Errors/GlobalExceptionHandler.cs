using ClyvoVet.API.Domain.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClyvoVet.API.Infrastructure.Errors
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title) = exception switch
            {
                NoContentException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
                ArgumentException or FormatException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
                DbUpdateException => (StatusCodes.Status409Conflict, "Conflito de persistência"),
                _ => (StatusCodes.Status500InternalServerError, "Erro interno da aplicação")
            };

            if (statusCode >= 500)
                _logger.LogError(exception, "Erro não tratado. TraceId: {TraceId}", httpContext.TraceIdentifier);
            else
                _logger.LogWarning(exception, "Erro tratado globalmente. TraceId: {TraceId}", httpContext.TraceIdentifier);

            httpContext.Response.StatusCode = statusCode;

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode switch
                {
                    StatusCodes.Status500InternalServerError => "Ocorreu um erro inesperado durante o processamento da requisição.",
                    StatusCodes.Status409Conflict => "A operação não pôde ser concluída devido a um conflito de persistência ou integridade dos dados.",
                    _ => exception.Message
                },
                Instance = httpContext.Request.Path
            };
            problem.Extensions["traceId"] = httpContext.TraceIdentifier;

            await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
            return true;
        }
    }
}
