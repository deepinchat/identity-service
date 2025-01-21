using Duende.IdentityServer.Models;

namespace Deepin.Identity.Server.Constants;

public static class ApiConstants
{
    public class IdentityApi
    {
        public static string Name => "users";
        public static ApiScope[] Scopes =>
            [
                new ApiScope("identity.user", "Users API"),
                new ApiScope("identity.ro", "Identity Read Only"),
                new ApiScope("identity.admin", "Identity Admin"),
            ];
    }
    public class StorageApi
    {
        public static string Name => "storage";
        public static ApiScope[] Scopes =>
            [
                new ApiScope("storage")
            ];
    }
}