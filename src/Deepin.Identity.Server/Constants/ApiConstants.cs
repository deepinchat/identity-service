using Duende.IdentityServer.Models;

namespace Deepin.Identity.Server.Constants;
public static class ApiConstants
{
    public class IdentityApi
    {
        public static string Name => "identity";
        public static ApiScope[] Scopes =>
            [
                new ApiScope("identity.server", "Identity server API scope"),
                new ApiScope("identity.client", "Identity client API scope"),
                new ApiScope("identity.user", "Identity user API scope"),
                new ApiScope("identity.role", "Identity role API scope")
            ];
    }

    public class StorageApi
    {
        public static string Name => "storage";
        public static ApiScope[] Scopes =>
            [
                new ApiScope("download", "Download files"),
                new ApiScope("upload", "Upload files"),
                new ApiScope("storage.delete", "Delete files"),
            ];
    }

    public class ChattingApi
    {
        public static string Name => "chat";
        public static ApiScope[] Scopes =>
            [
                new ApiScope("chat", "Chatting API scope"),
            ];
    }

    public class MessageApi
    {
        public static string Name => "message";
        public static ApiScope[] Scopes =>
            [
                new ApiScope("message", "Message API scope"),
            ];
    }

    public class EmailingApi
    {
        public static string Name => "email";
        public static ApiScope[] Scopes =>
            [
                new ApiScope("email.ro", "Send email"),
                new ApiScope("email.rw", "Send and receive email"),
            ];
    }

    public class PresenceApi
    {
        public static string Name => "presence";
        public static ApiScope[] Scopes =>
            [
                new ApiScope("presence", "User presence"),
            ];
    }
    public class WebBffApi
    {
        public static string Name => "webbff";
        public static ApiScope[] Scopes =>
            [
                new ApiScope("webbff", "Web BFF API scope"),
            ];
    }
}