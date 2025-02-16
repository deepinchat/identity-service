using Deepin.Identity.Application.Models.Users;

namespace Deepin.Identity.Application.Queries;

public interface IUserQueries
{
    Task<UserProfile> GetUserByIdAsync(string id);
    Task<IEnumerable<UserProfile>> GetUsersAsync(string[] ids);
}
