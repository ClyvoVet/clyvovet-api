using ClyvoVet.API.Application.Interfaces;
using ClyvoVet.API.Application.UseCases;
using ClyvoVet.API.Domain.Interfaces;
using ClyvoVet.API.Infrastructure.Data;
using ClyvoVet.API.Infrastructure.Data.Repositories;
using ClyvoVet.API.Infrastructure.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Oracle.EntityFrameworkCore;

namespace ClyvoVet.API.Infrastructure.IoC
{
    public static class Bootstrap
    {
        public static IServiceCollection AddClyvoVetInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("OracleDbConnection") ?? string.Empty;

            services.AddDbContext<ApplicationContext>(options =>
            {
                options.UseOracle(
                    connectionString,
                    oracleOptions => oracleOptions.UseOracleSQLCompatibility(
                        OracleSQLCompatibility.DatabaseVersion21));
            });

            services.AddTransient<IPetRepository, PetRepository>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IPetUseCase, PetUseCase>();
            services.AddTransient<IUserUseCase, UserUseCase>();
            services.AddSingleton<ApiMetrics>();

            return services;
        }
    }
}
