using Deepin.Domain;
using Deepin.Identity.Application.Commands.Users;
using Deepin.Identity.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Deepin.Identity.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController(
        IMediator mediator,
        IUserQueries userQueries,
        IUserContext userContext) : ControllerBase
    {
        private readonly IMediator _mediator = mediator;
        private readonly IUserQueries _userQueries = userQueries;
        private readonly IUserContext _userContext = userContext;
        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var userProfile = await _userQueries.GetUserByIdAsync(_userContext.UserId);
            if (userProfile == null)
            {
                return NotFound();
            }
            return Ok(userProfile);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAsync(string id)
        {
            var userProfile = await _userQueries.GetUserByIdAsync(id);
            if (userProfile == null)
            {
                return NotFound();
            }
            return Ok(userProfile);
        }

        [HttpGet("batch")]
        public async Task<IActionResult> BatchGetAsync([FromQuery] string[] ids)
        {
            var userProfiles = await _userQueries.GetUsersAsync(ids);
            return Ok(userProfiles);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] UpdateUserProfileCommand command)
        {
            var user = await _userQueries.GetUserByIdAsync(command.Id);
            if (user == null)
            {
                return NotFound();
            }
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
