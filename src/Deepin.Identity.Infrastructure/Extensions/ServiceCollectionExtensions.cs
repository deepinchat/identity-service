using Deepin.Identity.Infrastructure.Configurations;
using Duende.IdentityServer.EntityFramework.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Deepin.Identity.Infrastructure.Extensions;
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, AppSettings appSettings)
    {
        return services
             .AddSingleton(appSettings)
             .AddDbContexts(appSettings)
             .AddCaching(appSettings);
    }
    private static IServiceCollection AddDbContexts(this IServiceCollection services, AppSettings appSettings)
    {
        services.AddDbContext<IdentityContext>(options =>
        {
            options.UseNpgsql(appSettings.IdentityDbConnection, sql =>
            {
                sql.EnableRetryOnFailure(3);
            });
        }, ServiceLifetime.Scoped);
        services.AddConfigurationDbContext<ConfigurationContext>(options =>
        {
            options.ConfigureDbContext = builder => builder.UseNpgsql(appSettings.ConfigurationDbConnection, sql =>
            {
                sql.EnableRetryOnFailure(3);
            });
        });
        services.AddOperationalDbContext<PersistedGrantContext>(options =>
        {
            options.ConfigureDbContext = builder => builder.UseNpgsql(appSettings.PersistedGrantDbConnection, sql =>
            {
                sql.EnableRetryOnFailure(3);
            });
        });
        return services;
    }
    private static IServiceCollection AddCaching(this IServiceCollection services, AppSettings appSettings)
    {
        if (appSettings.UseRedisCache)
        {

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = appSettings.RedisConnection;
            });
        }
        else
        {
            services.AddMemoryCache();
        }
        return services;
    }
}
