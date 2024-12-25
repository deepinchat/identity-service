using Duende.IdentityServer.Models;

namespace Deepin.Identity.Web.Constants;

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
    public class FileApi
    {
        public static string Name => "files";
        public static ApiScope[] Scopes =>
            [
                new ApiScope("file.download", "Download File"),
                new ApiScope("file.upload", "Upload File"),
                new ApiScope("file.admin", "File Admin"),
            ];
    }
}