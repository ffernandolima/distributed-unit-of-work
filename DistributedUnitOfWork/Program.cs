using DistributedUnitOfWork.Abstractions;
using DistributedUnitOfWork.Repositories;
using DistributedUnitOfWork.Seeders;
using DistributedUnitOfWork.Services;
using DistributedUnitOfWork.UnitOfWorks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DistributedUnitOfWork;

public class Program
{
    public static async Task Main(string[] _)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var services = new ServiceCollection()
            // Register configuration
            .AddSingleton<IConfiguration>(configuration)

            // Register connection objects using connection strings from appsettings.json
            .AddScoped(_ => new SqlConnection(configuration.GetConnectionString("MsSql")))
            .AddScoped(_ => new NpgsqlConnection(configuration.GetConnectionString("Postgres")))

            // Register unit of work implementations (using keyed DI extensions)
            .AddKeyedScoped<IUnitOfWork, MsSqlUnitOfWork>("MsSql")
            .AddKeyedScoped<IUnitOfWork, NpgsqlUnitOfWork>("Npgsql")
            .AddKeyedScoped<IUnitOfWork, DistributedLinkedUnitOfWork>("Distributed")

            // Register database seeders
            .AddSingleton<IDbSeeder, MsSqlDbSeeder>()
            .AddSingleton<IDbSeeder, NpgsqlDbSeeder>()

            // Register repositories
            .AddScoped<IMsSqlRepository, MsSqlRepository>()
            .AddScoped<INpgsqlRepository, NpgsqlRepository>()

            // Register services
            .AddScoped<IDistributedService, DistributedService>();

        await using var serviceProvider = services.BuildServiceProvider();

        await RunApplicationAsync(serviceProvider);
    }

    private static async Task RunApplicationAsync(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        await using var serviceScope = serviceProvider.CreateAsyncScope();

        // Execute seeders
        var dbSeeders = serviceScope.ServiceProvider.GetServices<IDbSeeder>();

        foreach (var dbSeeder in dbSeeders)
        {
            await dbSeeder.Seed();
        }

        // Execute business logic
        var distributedService = serviceScope.ServiceProvider.GetRequiredService<IDistributedService>();

        await distributedService.ProcessData();
    }
}