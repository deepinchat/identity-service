namespace Deepin.Identity.Server.Models.Accounts;

public class LoginWith2faRequest
{
    public required string TwoFactorCode { get; set; }
    public bool RememberLogin { get; set; }
    public bool RememberMachine { get; set; }
}
