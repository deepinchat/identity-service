using System;
using Deepin.ServiceDefaults.Extensions;
using Deepin.EventBus.RabbitMQ;
using Deepin.Identity.Infrastructure.Configurations;
using Deepin.Identity.Infrastructure.Extensions;
using Deepin.Identity.Application.Extensions;

namespace Deepin.Identity.API.Extensions;

public static class HostExtensions
{
    public static WebApplicationBuilder AddApplicationService(this WebApplicationBuilder builder)
    {
        var appSettings = new AppSettings()
        {
            IdentityDbConnection = builder.Configuration.GetConnectionString("IdentityDb") ?? throw new ArgumentNullException("IdentityDbConnection must be not null."),
            ConfigurationDbConnection = builder.Configuration.GetConnectionString("ConfigurationDb") ?? throw new ArgumentNullException("ConfigurationDbConnection must be not null."),
            PersistedGrantDbConnection = builder.Configuration.GetConnectionString("PersistedGrantDb") ?? throw new ArgumentNullException("PersistedGrantDbConnection must be not null."),
            RedisConnection = builder.Configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException("RedisConnection")
        };

        builder.AddServiceDefaults();

        builder.Services
        .AddInfrastructure(appSettings)
        .AddApplication()
        .AddEventBusRabbitMQ(
            builder.Configuration.GetSection(RabbitMqOptions.ConfigurationKey).Get<RabbitMqOptions>() ?? throw new ArgumentNullException("RabbitMQ"),
            assembly: typeof(HostExtensions).Assembly)
        .AddDefaultCache(new Deepin.Infrastructure.Caching.RedisCacheOptions()
        {
            ConnectionString = appSettings.RedisConnection
        })
        .AddDefaultUserContexts();

        return builder;
    }
    public static WebApplication UseApplicationService(this WebApplication app)
    {
        app.UseServiceDefaults();

        app.MapGet("/api/about", () => new
        {
            Name = "Deepin.Identity.API",
            Version = "1.0.0",
            DeepinEnv = app.Configuration["DEEPIN_ENV"],
            app.Environment.EnvironmentName
        });
        return app;
    }
}
