using System.ComponentModel.DataAnnotations;

namespace Deepin.Identity.Server.Models.Accounts;

public class ResetPasswordRequest : ForgotPasswordRequest
{
    [Required]
    [StringLength(32, ErrorMessage = "The password must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    public required string Password { get; set; }
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public required string ConfirmPassword { get; set; }
    [Required]
    [StringLength(6, ErrorMessage = "The verify code must be at least {2} and at max {1} characters long.", MinimumLength = 4)]
    public required string Code { get; set; }
}
