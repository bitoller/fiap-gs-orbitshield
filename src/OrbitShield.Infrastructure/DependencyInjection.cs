using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrbitShield.Application.Abstractions;
using OrbitShield.Application.Repositories;
using OrbitShield.Infrastructure.Persistence;
using OrbitShield.Infrastructure.Repositories;
using OrbitShield.Infrastructure.Security;

namespace OrbitShield.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = OracleConnectionStringFactory.Create(configuration.GetConnectionString("Oracle"));

        services.AddDbContext<OrbitShieldDbContext>(options => options.UseOracle(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISatelliteRepository, SatelliteRepository>();
        services.AddScoped<IMissionRepository, MissionRepository>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        return services;
    }
}
