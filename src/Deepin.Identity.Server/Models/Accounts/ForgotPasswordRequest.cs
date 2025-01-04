using System.ComponentModel.DataAnnotations;

namespace Deepin.Identity.Server.Models.Accounts;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }
}
