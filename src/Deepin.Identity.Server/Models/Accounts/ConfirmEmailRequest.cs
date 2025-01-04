namespace Deepin.Identity.Server.Models.Accounts;

public class ConfirmEmailRequest
{
    public required string UserId { get; set; }
    public required string Code { get; set; }
}
