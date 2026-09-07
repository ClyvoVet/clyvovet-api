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
        public Mock<IUserUseCase> UserUseCaseMock { get; } = new();

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
                // Mesmo padrao do projeto-modelo: nos testes de controller,
                // substituimos apenas os UseCases por mocks.
                services.RemoveAll(typeof(IPetUseCase));
                services.RemoveAll(typeof(IUserUseCase));

                services.AddSingleton(PetUseCaseMock.Object);
                services.AddSingleton(UserUseCaseMock.Object);
            });
        }
    }

    [CollectionDefinition("API Integration")]
    public class ApiIntegrationCollection : ICollectionFixture<CustomWebApplicationFactory>
    {
    }
}
