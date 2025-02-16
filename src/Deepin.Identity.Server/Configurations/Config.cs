using Deepin.Identity.Server.Constants;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace Deepin.Identity.Server.Configurations;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };
    public static IEnumerable<ApiResource> ApiResources => [
        new ApiResource(ApiConstants.IdentityApi.Name){
            Scopes = ApiConstants.IdentityApi.Scopes.Select(s=>s.Name).ToArray()
        },
        new ApiResource(ApiConstants.StorageApi.Name){
            Scopes = ApiConstants.StorageApi.Scopes.Select(s=>s.Name).ToArray()
        },
        new ApiResource(ApiConstants.ChattingApi.Name){
            Scopes = ApiConstants.ChattingApi.Scopes.Select(s=>s.Name).ToArray()
        },
        new ApiResource(ApiConstants.MessageApi.Name){
            Scopes = ApiConstants.MessageApi.Scopes.Select(s=>s.Name).ToArray()
        },
        new ApiResource(ApiConstants.EmailingApi.Name){
            Scopes = ApiConstants.EmailingApi.Scopes.Select(s=>s.Name).ToArray()
        },
        new ApiResource(ApiConstants.PresenceApi.Name){
            Scopes = ApiConstants.PresenceApi.Scopes.Select(s=>s.Name).ToArray()
        },
        new ApiResource(ApiConstants.WebBffApi.Name){
            Scopes = ApiConstants.WebBffApi.Scopes.Select(s=>s.Name).ToArray()
        },
        ];
    public static IEnumerable<ApiScope> ApiScopes => ApiResources.SelectMany(r => r.Scopes).Select(s => new ApiScope(s));
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "postman",
                ClientName = "Postman Client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                ClientSecrets = {new Secret("secret".Sha256())},
                AllowedScopes =
                [
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    .. ApiScopes.Select(s => s.Name)
                ],
            },
            new Client
            {
                ClientId = "deepinweb",
                ClientName = "Deepin Web Client",
                AllowedGrantTypes = GrantTypes.Code,
                AllowAccessTokensViaBrowser = true,
                RequirePkce = true,
                RequireClientSecret = false,
                RequireConsent = false,
                RedirectUris = {"http://localhost:4200/callback/signin","https://deepin.chat/callback/signin","https://deepin.me/callback/signin"},
                PostLogoutRedirectUris = {"http://localhost:4200/callback/signout","https://deepin.chat/callback/signout","https://deepin.me/callback/signout"},
                AllowedCorsOrigins = {"http://localhost:4200" , "https://deepin.chat","https://deepin.me"},
                AllowedScopes =
                [
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "upload",
                    "download",
                    "chat",
                    "message",
                    "presence",
                    "webbff",
                    "identity.user"
                ],
                AccessTokenLifetime = 3600,
                IdentityTokenLifetime = 1800,
            },
            new Client
            {
                ClientId = "deepinswaggerui",
                ClientName = "Deepin Swagger UI",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = {"http://localhost:5000/swagger/oauth2-redirect.html","https://localhost:5000/swagger/oauth2-redirect.html"},
                AllowedScopes = [
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    .. ApiScopes.Select(s => s.Name)
                ],
                RequireConsent = false,
            }
        };
}
