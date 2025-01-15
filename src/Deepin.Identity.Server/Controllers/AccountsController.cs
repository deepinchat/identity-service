using Deepin.Identity.Domain.Entities;
using Deepin.Identity.Server.Configurations;
using Deepin.Identity.Server.Infrastructure.Services;
using Deepin.Identity.Server.Models.Accounts;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Deepin.Identity.Server.Controllers;

[Route("api/v1/[controller]")]
[ApiController]
[Authorize]
public class AccountsController(
    ILogger<AccountsController> logger,
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IEventService eventService,
    IBus bus) : ControllerBase
{
    private readonly IEventService _events = eventService;
    private readonly ILogger<AccountsController> _logger = logger;
    private readonly UserManager<User> _userManager = userManager;
    private readonly SignInManager<User> _signInManager = signInManager;
    private readonly IBus _bus = bus;

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, string? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            var user = await GetUserByLogin(request.UserName);

            if (user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user.UserName!, request.Password, request.RememberLogin, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id.ToString(), user.UserName));
                    return Ok();
                }
                else if (result.IsNotAllowed)
                {
                    return StatusCode(302, RoutersConfig.ConfirmEmail(user.Id, returnUrl ?? "/"));
                }
                else if (result.RequiresTwoFactor)
                {
                    return StatusCode(302, RoutersConfig.LoginWithTwoFactor(request.RememberLogin, returnUrl ?? "/"));
                }
                await _events.RaiseAsync(new UserLoginFailureEvent(user.UserName, AccountOptions.InvalidCredentials.Message));
            }
            ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentials.Key);
        }
        return BadRequest(ModelState);
    }
    [HttpPost("Send2FACode")]
    [AllowAnonymous]
    public async Task<IActionResult> Send2FACode()
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Unable to load two-factor authentication user.");
            return BadRequest(ModelState);
        }
        var code = await _userManager.GenerateTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider);
        await _bus.Publish(EmailBuilder.Build2FAEmailEvent(user.Email!, code));
        return Ok();
    }

    [HttpGet("ExternalCallback")]
    [AllowAnonymous]
    public async Task<IActionResult> ExternalCallback()
    {
        var loginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (loginInfo == null)
        {
            ModelState.AddModelError(string.Empty, AccountOptions.InvalidExternalLoginErrorMessage);
        }
        else
        {
            var user = await _userManager.FindByLoginAsync(loginInfo.LoginProvider, loginInfo.ProviderKey);
            if (user == null)
            {
                user = new User()
                {
                    UserName = Guid.NewGuid().ToString(),
                };
                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    var addLoginResult = await _userManager.AddLoginAsync(user, loginInfo);
                    if (!addLoginResult.Succeeded)
                    {
                        AddIdentityErrors(result);
                        await _userManager.DeleteAsync(user);
                    }
                }
                else
                    AddIdentityErrors(result);
            }
            if (ModelState.IsValid)
            {
                await _signInManager.SignOutAsync();
                await _signInManager.SignInAsync(user, isPersistent: false);
                return Ok();
            }
        }
        return BadRequest(ModelState);
    }
    [HttpPost("Register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var existUser = await _userManager.FindByEmailAsync(request.Email);
        if (existUser == null)
        {
            var user = new User { Email = request.Email, UserName = request.Email, CreatedAt = DateTime.UtcNow };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _bus.Publish(EmailBuilder.BuildRegisterVerificationEmailEvent(user.Email, code));
                return Ok(new { Id = user.Id });
            }
            else
            {
                AddIdentityErrors(result);
            }
        }
        else
        {
            ModelState.AddModelError(nameof(request.Email), AccountOptions.EmailIsTakenErrorMessage);
        }
        return BadRequest(ModelState);
    }
    [HttpGet("ResendEmailConfirmation")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendEmailConfirmation(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Verification email sent. Please check your email.");
            return BadRequest(ModelState);
        }
        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        await _bus.Publish(EmailBuilder.BuildRegisterVerificationEmailEvent(user.Email!, code));
        return Ok();

    }
    [HttpPost("ForgotPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            return Ok();
        }
        var code = await _userManager.GeneratePasswordResetTokenAsync(user);
        await _bus.Publish(EmailBuilder.BuildResetPasswordEmailEvent(user.Email!, code));
        return Ok();
    }
    [HttpPost("ResetPassword")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user != null)
        {
            await _userManager.ResetPasswordAsync(user, request.Code, request.Password);
        }
        return Ok();
    }
    [HttpPost("ConfirmEmail")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(ConfirmEmailRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{request.UserId}'.");
        }
        var result = await _userManager.ConfirmEmailAsync(user, request.Code);
        if (!result.Succeeded)
        {
            AddIdentityErrors(result);
            return BadRequest(ModelState);
        }
        return Ok();
    }
    [HttpPost("LoginWith2fa")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWith2fa([FromBody] LoginWith2faRequest request)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Unable to load two-factor authentication user.");
            return BadRequest(ModelState);
        }

        var authenticatorCode = request.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, request.RememberLogin, request.RememberMachine);
        if (result.Succeeded)
        {
            return Ok();
        }
        else if (result.IsLockedOut)
        {
            return StatusCode(302, RoutersConfig.Lockout);
        }
        else
        {
            ModelState.AddModelError(string.Empty, AccountOptions.InvalidAuthenticatorCode.Key);
            return BadRequest(ModelState);
        }
    }
    [HttpPost("Logout")]
    public async Task<IActionResult> Logout()
    {
        if (User?.Identity.IsAuthenticated == true)
        {
            // delete local authentication cookie
            await _signInManager.SignOutAsync();

            // raise the logout event
            await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
        }
        return NoContent();
    }
    [HttpGet("IsAuthorized")]
    public IActionResult IsAuthorized()
    {
        return Ok();
    }
    private async Task<User?> GetUserByLogin(string login)
    {
        var user = await _userManager.FindByNameAsync(login);
        return user ?? await _userManager.FindByEmailAsync(login);
    }
    private void AddIdentityErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }
}
