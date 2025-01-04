using Deepin.Identity.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Deepin.Identity.Server.Controllers
{
    public class ChallengeController(SignInManager<User> signInManager) : Controller
    {
        private readonly SignInManager<User> _signInManager = signInManager;

        [AllowAnonymous]
        [HttpGet]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = new Uri(new Uri($"{Request.Scheme}://{Request.Host}"), $"/callback/external-login?returnUrl={returnUrl}");
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl.ToString());
            return Challenge(properties, provider);
        }
    }
}
