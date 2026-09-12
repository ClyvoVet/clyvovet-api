using System.Diagnostics;
using System.IO.Compression;
using ClyvoVet.API.Infrastructure.Data;
using ClyvoVet.API.Infrastructure.IoC;
using ClyvoVet.API.Infrastructure.Observability;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

var logDirectory = Path.Combine(AppContext.BaseDirectory, "logs");
Directory.CreateDirectory(logDirectory);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "ClyvoVet.API")
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine(logDirectory, "api-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [CorrelationId:{CorrelationId}] [TraceId:{TraceId}] {SourceContext}{NewLine}  {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddSerilog();

Log.Information("Logging estruturado inicializado. Arquivos de log em {LogDirectory}", logDirectory);

builder.Services.AddClyvoVetInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.ExampleFilters();
});
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy("API em execucao."),
        tags: ["live"])
    .AddOracle(
        connectionString: builder.Configuration.GetConnectionString("OracleDbConnection") ?? string.Empty,
        name: "oracle",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["db"]);

var enableConsoleExporter = !bool.TryParse(
    builder.Configuration["Observability:EnableConsoleExporter"],
    out var consoleExporterEnabled) || consoleExporterEnabled;

var openTelemetry = builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("ClyvoVet.API"));

openTelemetry.WithTracing(tracing =>
{
    tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddSource("ClyvoVet.Application")
        .AddSource("ClyvoVet.Infrastructure");

    if (enableConsoleExporter)
        tracing.AddConsoleExporter();
});

openTelemetry.WithMetrics(metrics =>
{
    metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter(ApiMetrics.MeterName);

    if (enableConsoleExporter)
        metrics.AddConsoleExporter();
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.Use(async (context, next) =>
{
    const string correlationHeader = "X-Correlation-ID";
    var correlationId = context.Request.Headers[correlationHeader].FirstOrDefault();

    if (string.IsNullOrWhiteSpace(correlationId))
        correlationId = Guid.NewGuid().ToString("N");

    context.TraceIdentifier = correlationId;
    context.Response.Headers[correlationHeader] = correlationId;

    var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    using (LogContext.PushProperty("CorrelationId", correlationId))
    using (LogContext.PushProperty("TraceId", traceId))
    {
        await next();
    }
});

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, _, exception) =>
    {
        if (exception is not null || httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        return httpContext.Response.StatusCode >= 400
            ? LogEventLevel.Warning
            : LogEventLevel.Information;
    };
});

app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();
    try
    {
        await next();
    }
    finally
    {
        stopwatch.Stop();
        var metrics = context.RequestServices.GetRequiredService<ApiMetrics>();
        metrics.Record(
            context.Request.Method,
            context.Request.Path.Value ?? "/",
            context.Response.StatusCode,
            stopwatch.Elapsed.TotalMilliseconds);
    }
});

app.UseAuthorization();
app.UseResponseCompression();

app.MapGet("/metrics", (ApiMetrics metrics) => Results.Ok(metrics.GetSnapshot()));

app.MapControllers();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

    const int maxRetries = 6;
    const int retryDelaySeconds = 20;

    for (var attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            dbContext.Database.Migrate();
            Log.Information("Migrations do banco de dados aplicadas/verificadas com sucesso");
            break;
        }
        catch (Exception ex) when (attempt < maxRetries)
        {
            Log.Warning(ex,
                "Tentativa {Attempt}/{Max} de aplicar migrations falhou (Oracle ainda inicializando). " +
                "Proxima tentativa em {Delay}s...",
                attempt, maxRetries, retryDelaySeconds);
            Thread.Sleep(TimeSpan.FromSeconds(retryDelaySeconds));
        }
        catch (Exception ex)
        {
            Log.Error(ex,
                "Todas as {Max} tentativas de aplicar migrations falharam. " +
                "A API iniciara sem garantia de schema atualizado.",
                maxRetries);
        }
    }
}

app.Run();

public partial class Program { }