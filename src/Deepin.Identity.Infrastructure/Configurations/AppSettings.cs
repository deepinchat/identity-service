namespace Deepin.Identity.Infrastructure.Configurations;
public class AppSettings
{
    public string IdentityDbConnection { get; set; } = string.Empty;
    public string ConfigurationDbConnection { get; set; } = string.Empty;
    public string PersistedGrantDbConnection { get; set; } = string.Empty;
    public string RedisConnection { get; set; } = string.Empty;
    public bool UseRedisCache => !string.IsNullOrEmpty(RedisConnection);
}
