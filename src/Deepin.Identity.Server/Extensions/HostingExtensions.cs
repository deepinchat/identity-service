using Deepin.Identity.Application.Extensions;
using Deepin.Identity.Domain.Entities;
using Deepin.Identity.Infrastructure;
using Deepin.Identity.Infrastructure.Configurations;
using Deepin.Identity.Infrastructure.Extensions;
using Deepin.Identity.Server.Infrastructure.Filters;
using Deepin.Identity.Server.Setup;
using Duende.IdentityServer;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using StackExchange.Redis;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace Deepin.Identity.Server.Extensions;

public static class HostingExtensions
{
    private const string ALLOW_ANY_CORS_POLICY = "allow_any";
    public static WebApplicationBuilder AddApplicationService(this WebApplicationBuilder builder)
    {
        var appSettings = new AppSettings()
        {
            IdentityDbConnection = builder.Configuration.GetConnectionString("IdentityDb") ?? throw new ArgumentNullException("IdentityDbConnection must be not null."),
            ConfigurationDbConnection = builder.Configuration.GetConnectionString("ConfigurationDb") ?? throw new ArgumentNullException("ConfigurationDbConnection must be not null."),
            PersistedGrantDbConnection = builder.Configuration.GetConnectionString("PersistedGrantDb") ?? throw new ArgumentNullException("PersistedGrantDbConnection must be not null."),
            RedisConnection = builder.Configuration.GetConnectionString("Redis") ?? throw new ArgumentNullException("RedisConnection must be not null.")
        };
        builder.Services
            .AddCustomCookiePolicy()
            .AddCustomMvc()
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
    internal static IServiceCollection AddCustomMvc(this IServiceCollection services)
    {
        services
          .AddControllers(options =>
          {
              options.Filters.Add<HttpGlobalExceptionFilter>();
          })
          .AddNewtonsoftJson(options =>
          {
              var jsonSettings = options.SerializerSettings;
              jsonSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
              jsonSettings.Converters = [
                  new StringEnumConverter(){ NamingStrategy = new CamelCaseNamingStrategy()}
                  ];
              jsonSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
              jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
              JsonConvert.DefaultSettings = () => jsonSettings;
          });

        services.AddHealthChecks().AddCheck("default", () => HealthCheckResult.Healthy());
        services.AddOpenApi();
        services.AddCors(options =>
        {
            options.AddPolicy(ALLOW_ANY_CORS_POLICY, builder =>
            {
                builder.SetIsOriginAllowed(_ => true)
                       .AllowAnyMethod()
                       .AllowAnyHeader()
                       .AllowCredentials();
            });
        });
        return services;
    }
    private static IServiceCollection AddCustomCookiePolicy(this IServiceCollection services)
    {
        services.Configure<CookiePolicyOptions>(options =>
        {
            options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
            options.OnAppendCookie = (cookieContext) =>
            {
                cookieContext.CookieOptions.SameSite = SameSiteMode.Unspecified;
            };
            options.OnDeleteCookie = (cookieContext) =>
            {
                cookieContext.CookieOptions.SameSite = SameSiteMode.Unspecified;
            };
        });
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
            options.Tokens.ChangeEmailTokenProvider = TokenOptions.DefaultEmailProvider;
            options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
        });

        services.ConfigureApplicationCookie(o =>
        {
            o.Cookie.Name = "deepin.identity";
            o.Events.OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
                else
                {
                    ctx.Response.Redirect(ctx.RedirectUri);
                }
                return Task.FromResult(0);
            };
            o.LoginPath = new PathString("/signin");
            o.LogoutPath = new PathString("/signout");
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
                 options.KeyManagement.Enabled = false;
             })
             .AddConfigurationStore<ConfigurationContext>()
             .AddOperationalStore<PersistedGrantContext>(options =>
             {
                 // this enables automatic token cleanup. this is optional.
                 options.EnableTokenCleanup = true;
                 options.TokenCleanupInterval = 3600; // interval in seconds (default is 3600)
             })
             .AddAspNetIdentity<User>();

        var authBuilder = services.AddAuthentication();
        if (configuration["GitHub"] is not null)
        {
            authBuilder.AddGitHub(
                configuration["GitHub:ClientId"] ?? throw new ArgumentNullException("Github ClientId must be not null"),
                configuration["GitHub:ClientSecret"] ?? throw new ArgumentNullException("Github ClientSecret must be not null"));
        }
        if (configuration["Google"] is not null)
        {
            authBuilder.AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google  
                options.ClientId = configuration["Google:ClientId"] ?? throw new ArgumentNullException("Google ClientId must be not null");
                options.ClientSecret = configuration["Google:ClientSecret"] ?? throw new ArgumentNullException("Google ClientSecret must be not null");
            });
        }


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
    public static AuthenticationBuilder AddGitHub(this AuthenticationBuilder builder, string clientId, string clientSecret)
    {
        // You must first create an app with GitHub and add its ID and Secret to your user-secrets.
        // https://github.com/settings/applications/
        // https://docs.github.com/en/developers/apps/authorizing-oauth-apps
        return builder.AddOAuth("GitHub", "GitHub", o =>
        {
            o.ClientId = clientId;
            o.ClientSecret = clientSecret;
            o.CallbackPath = new PathString("/signin-github");
            o.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
            o.TokenEndpoint = "https://github.com/login/oauth/access_token";
            o.UserInformationEndpoint = "https://api.github.com/user";
            o.ClaimsIssuer = "OAuth2-Github";
            o.SaveTokens = true;
            // Retrieving user information is unique to each provider.
            o.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
            o.ClaimActions.MapJsonKey(ClaimTypes.Name, "login");
            o.ClaimActions.MapJsonKey("urn:github:name", "name");
            o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
            o.ClaimActions.MapJsonKey("urn:github:url", "url");
            o.ClaimActions.MapJsonKey(JwtClaimTypes.Picture, "avatar_url");
            o.Events = new OAuthEvents
            {
                OnCreatingTicket = async context =>
                {
                    // Get the GitHub user
                    var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
                    response.EnsureSuccessStatusCode();

                    using (var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync()))
                    {
                        context.RunClaimActions(user.RootElement);
                    }
                }
            };
        });
    }
    public static WebApplication ConfigureApplicationService(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.MapOpenApi();
        }
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });
        app.UseDefaultFiles();
        app.MapStaticAssets();

        // Configure the HTTP request pipeline.
        app.UseHttpsRedirection();

        app.UseCookiePolicy();
        app.UseCors(ALLOW_ANY_CORS_POLICY);

        app.UseIdentityServer();
        app.UseAuthorization();


        app.MapHealthChecks("health");
        app.MapControllers();

        app.MapFallbackToFile("/index.html");

        return app;
    }
}
