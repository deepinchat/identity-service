using Deepin.Identity.Infrastructure;
using Deepin.Identity.Web.Configurations;
using Duende.IdentityServer.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;

namespace Deepin.Identity.Web.Setup;

public class ConfigurationDbSeeder(IServiceProvider serviceProvider, ConfigurationContext db)
{
    private readonly ConfigurationContext _db = db;

    public async Task SeedAsync()
    {
        if (!await _db.IdentityResources.AnyAsync())
        {
            Config.IdentityResources.ToList().ForEach(item =>
            {
                _db.IdentityResources.Add(item.ToEntity());
            });
            await _db.SaveChangesAsync();
        }
        if (!await _db.ApiScopes.AnyAsync())
        {
            Config.ApiScopes.ToList().ForEach(item =>
            {
                _db.ApiScopes.Add(item.ToEntity());
            });
            await _db.SaveChangesAsync();
        }
        if (!await _db.ApiResources.AnyAsync())
        {
            Config.ApiResources.ToList().ForEach(item =>
            {
                _db.ApiResources.Add(item.ToEntity());
            });
            await _db.SaveChangesAsync();
        }
        if (!await _db.Clients.AnyAsync())
        {
            Config.Clients.ToList().ForEach(item =>
            {
                _db.Clients.Add(item.ToEntity());
            });
            await _db.SaveChangesAsync();
        }
    }
}
