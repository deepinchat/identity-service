using System;
using System.ComponentModel.DataAnnotations;

namespace Deepin.Identity.Server.Models.Accounts;

public class LoginRequest
{
    [Required]
    public required string UserName { get; set; }
    [StringLength(32, ErrorMessage = "The password must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    public required string Password { get; set; }
    public bool RememberLogin { get; set; }

}
