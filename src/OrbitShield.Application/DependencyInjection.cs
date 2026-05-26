using Microsoft.Extensions.DependencyInjection;
using OrbitShield.Application.Auth;
using OrbitShield.Application.Mission;
using OrbitShield.Application.Orbital;
using OrbitShield.Application.Satellites;

namespace OrbitShield.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISatelliteService, SatelliteService>();
        services.AddScoped<IMissionControlService, MissionControlService>();
        services.AddScoped<IOrbitalScenarioService, OrbitalScenarioService>();

        return services;
    }
}
