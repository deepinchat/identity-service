namespace Deepin.Identity.Server.Configurations;

public class RoutersConfig
    {
        public static string ConfirmEmail(string userId, string returnUrl) => $"/confirm-email?id={userId}&returnUrl={returnUrl ?? "/"}";
        public static string LoginWithTwoFactor(bool remberMe, string returnUrl) => $"/login-2factor?remberMe={remberMe}&returnUrl={returnUrl ?? "/"}";
        public static string Lockout => "/lockout";
    }