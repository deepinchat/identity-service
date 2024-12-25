using Deepin.Identity.Application.Extensions;
using Deepin.Identity.Domain.Entities;
using Deepin.Identity.Infrastructure;
using Deepin.Identity.Infrastructure.Configurations;
using Deepin.Identity.Infrastructure.Extensions;
using Deepin.Identity.Web.Setup;
using Duende.IdentityServer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using StackExchange.Redis;
using System.Security.Cryptography.X509Certificates;

namespace Deepin.Identity.Web.Extensions;

public static class HostingExtensions
{
    public static WebApplicationBuilder AddApplicationService(this WebApplicationBuilder builder)
    {
        var appSettings = new AppSettings()
        {
            IdentityDbConnection = builder.Configuration.GetConnectionString("IdentityDb") ?? throw new ArgumentNullException("IdentityDbConnection must be not null."),
            ConfigurationDbConnection = builder.Configuration.GetConnectionString("ConfigurationDb") ?? throw new ArgumentNullException("ConfigurationDbConnection must be not null."),
            PersistedGrantDbConnection = builder.Configuration.GetConnectionString("PersistedGrantDb") ?? throw new ArgumentNullException("PersistedGrantDbConnection must be not null."),
            RedisConnection = builder.Configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException("RedisConnection must be not null.")
        };

        builder.Services.AddRazorPages();
        builder.Services
            .AddInfrastructure(appSettings)
            .AddApplication()
            .AddCustomIdentity()
            .AddCustomIdentityServer(builder.Environment, builder.Configuration)
            .AddCustomDataProtection(appSettings);

        builder.Services
            .AddMigration<IdentityContext>()
            .AddMigration<ConfigurationContext>((db, sp) => new ConfigurationDbSeeder(sp, db).SeedAsync())
            .AddMigration<PersistedGrantContext>();
        return builder;
    }
    private static IServiceCollection AddCustomDataProtection(this IServiceCollection services, AppSettings appSettings)
    {
        services.AddDataProtection(opts =>
        {
            opts.ApplicationDiscriminator = "identity-service";
        }).PersistKeysToStackExchangeRedis(ConnectionMultiplexer.Connect(appSettings.RedisConnection), "IdentityService.DataProtection.Keys");
        return services;
    }
    private static IServiceCollection AddCustomIdentity(this IServiceCollection services)
    {
        services
        .AddIdentityApiEndpoints<User>()
        .AddRoles<Domain.Entities.Role>()
        .AddEntityFrameworkStores<IdentityContext>();

        services.Configure<IdentityOptions>(options =>
        {
            options.SignIn.RequireConfirmedAccount = false;
            options.User.RequireUniqueEmail = true;
            options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
        });

        return services;
    }
    private static IServiceCollection AddCustomIdentityServer(this IServiceCollection services, IWebHostEnvironment env, IConfiguration configuration)
    {
        var idsvBuilder = services
             .AddIdentityServer(options =>
             {
                 options.Events.RaiseErrorEvents = true;
                 options.Events.RaiseInformationEvents = true;
                 options.Events.RaiseFailureEvents = true;
                 options.Events.RaiseSuccessEvents = true;

                 // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                 options.EmitStaticAudienceClaim = true;
             })
             .AddConfigurationStore<ConfigurationContext>()
             .AddOperationalStore<PersistedGrantContext>(options =>
             {
                 // this enables automatic token cleanup. this is optional.
                 options.EnableTokenCleanup = true;
                 options.TokenCleanupInterval = 3600; // interval in seconds (default is 3600)
             })
             .AddAspNetIdentity<User>();

        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
            });

        if (env.IsDevelopment())
        {
            idsvBuilder.AddDeveloperSigningCredential();
        }
        else
        {
            idsvBuilder.AddSigningCredential(X509CertificateLoader.LoadPkcs12FromFile(
                configuration["CERT_PATH"] ?? throw new ArgumentNullException("Certificate file path must be not null."),
                configuration["CERT_PASSWORD"] ?? throw new ArgumentNullException("Certificate file password must be not null.")));
        }
        return services;
    }
    public static WebApplication ConfigureApplicationService(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();

        app.MapRazorPages()
            .RequireAuthorization();

        return app;
    }
}
