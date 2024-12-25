using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;

namespace Deepin.Identity.Web.Infrastructure.Services;

public class ProfileService : IProfileService
{
    public Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        throw new NotImplementedException();
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        throw new NotImplementedException();
    }
}
