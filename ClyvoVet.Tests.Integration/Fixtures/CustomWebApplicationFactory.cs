using ClyvoVet.API.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace ClyvoVet.Tests.Integration
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        public Mock<IPetUseCase> PetUseCaseMock { get; } = new();
        public Mock<ITutorUseCase> TutorUseCaseMock { get; } = new();
        public Mock<IConsultaUseCase> ConsultaUseCaseMock { get; } = new();
        public Mock<IMedicacaoUseCase> MedicacaoUseCaseMock { get; } = new();
        public Mock<IHistoricoClinicoUseCase> HistoricoClinicoUseCaseMock { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:EnableConsoleExporter"] = "false",
                    ["Secretkey"] = "TEST_SECRET_KEY_CLYVOVET_12345678901234567890"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IPetUseCase));
                services.RemoveAll(typeof(ITutorUseCase));
                services.RemoveAll(typeof(IConsultaUseCase));
                services.RemoveAll(typeof(IMedicacaoUseCase));
                services.RemoveAll(typeof(IHistoricoClinicoUseCase));

                services.AddSingleton(PetUseCaseMock.Object);
                services.AddSingleton(TutorUseCaseMock.Object);
                services.AddSingleton(ConsultaUseCaseMock.Object);
                services.AddSingleton(MedicacaoUseCaseMock.Object);
                services.AddSingleton(HistoricoClinicoUseCaseMock.Object);

                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                    options.DefaultChallengeScheme = TestAuthHandler.Scheme;
                }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.Scheme, _ => { });
            });
        }
    }

    [CollectionDefinition("API Integration")]
    public class ApiIntegrationCollection : ICollectionFixture<CustomWebApplicationFactory>
    {
    }
}
