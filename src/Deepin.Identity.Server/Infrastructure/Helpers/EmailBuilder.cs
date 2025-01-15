using Deepin.EventBus.Events;

namespace Deepin.Identity.Server.Infrastructure.Services;

public static class EmailBuilder
{
    public static SendEmailIntegrationEvent BuildRegisterVerificationEmailEvent(string email, string code)
    {
        var bodyHtml = $@"
            <html>
                <head>
                    <title>Register Verification</title>
                </head>
                <body>
                    <h1>Register Verification</h1>
                    <p>Hi {email},</p>
                    <p>Thank you for registering. Please use the following code to verify your email address:</p>
                    <p><strong>{code}</strong></p>
                    <p>Best regards,</p>
                    <p>Deepin</p>
                </body>
            </html>";
        return new SendEmailIntegrationEvent(
            To: [email],
            Subject: "Register Verification",
            Body: bodyHtml,
            IsBodyHtml: true,
            CC: null);
    }

    public static SendEmailIntegrationEvent BuildResetPasswordEmailEvent(string email, string code)
    {
        var bodyHtml = $@"
            <html>
                <head>
                    <title>Reset Password</title>
                </head>
                <body>
                    <h1>Reset Password</h1>
                    <p>Hi {email},</p>
                    <p>Please use the following code to reset your password:</p>
                    <p><strong>{code}</strong></p>
                    <p>Best regards,</p>
                    <p>Deepin</p>
                </body>
            </html>";
        return new SendEmailIntegrationEvent(
            To: [email],
            Subject: "Reset Password",
            Body: bodyHtml,
            IsBodyHtml: true,
            CC: null);
    }
    public static SendEmailIntegrationEvent Build2FAEmailEvent(string email, string code)
    {
        var bodyHtml = $@"
            <html>
                <head>
                    <title>2FA Code</title>
                </head>
                <body>
                    <h1>2FA Code</h1>
                    <p>Hi {email},</p>
                    <p>Please use the following code to verify your identity:</p>
                    <p><strong>{code}</strong></p>
                    <p>Best regards,</p>
                    <p>Deepin</p>
                </body>
            </html>";
        return new SendEmailIntegrationEvent(
            To: [email],
            Subject: "2FA Code",
            Body: bodyHtml,
            IsBodyHtml: true,
            CC: null);
    }
}
