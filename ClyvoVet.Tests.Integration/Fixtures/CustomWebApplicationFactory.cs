using ClyvoVet.API.Application.Interfaces;
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

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Observability:EnableConsoleExporter"] = "false"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll(typeof(IPetUseCase));
                services.RemoveAll(typeof(ITutorUseCase));
                services.RemoveAll(typeof(IConsultaUseCase));
                services.RemoveAll(typeof(IMedicacaoUseCase));

                services.AddSingleton(PetUseCaseMock.Object);
                services.AddSingleton(TutorUseCaseMock.Object);
                services.AddSingleton(ConsultaUseCaseMock.Object);
                services.AddSingleton(MedicacaoUseCaseMock.Object);
            });
        }
    }

    [CollectionDefinition("API Integration")]
    public class ApiIntegrationCollection : ICollectionFixture<CustomWebApplicationFactory>
    {
    }
}
