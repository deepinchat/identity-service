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
        }];
    public static IEnumerable<ApiScope> ApiScopes =>
        ApiConstants.IdentityApi.Scopes.Concat(ApiConstants.StorageApi.Scopes);
    public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "postman",
                ClientName = "Postman Client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                ClientSecrets = {new Secret("secret".Sha256())},
                AllowedScopes = {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "identity.admin",
                }
            },
            new Client
            {
                ClientId = "deepinswaggerui",
                ClientName = "Deepin Swagger UI",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,
                RedirectUris = {"https://localhost:5000/swagger/oauth2-redirect.html"},
                AllowedScopes = {
                    "storage"
                },
                RequireConsent = false,
            }
        };
}
