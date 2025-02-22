using System.Security.Claims;
using Dapper;
using Deepin.Identity.Application.Models.Users;
using Deepin.Identity.Infrastructure.Configurations;
using IdentityModel;
using Npgsql;

namespace Deepin.Identity.Application.Queries;

public class UserQueries(AppSettings appSettings) : QueryBase, IUserQueries
{
    private readonly AppSettings _appSettings = appSettings;

    public async Task<UserProfile> GetUserByIdAsync(string id)
    {
        using var connection = new NpgsqlConnection(_appSettings.IdentityDbConnection);
        connection.Open();
        var sql = @"SELECT
                u.""Id"", u.""UserName"", u.""Email"", u.""PhoneNumber"", u.""CreatedAt"", u.""UpdatedAt"",
                user_claim.""ClaimType"", user_claim.""ClaimValue""
                FROM ""users"" u
                LEFT JOIN ""user_claims"" user_claim ON user_claim.""UserId"" = u.""Id""
                WHERE u.""Id"" = @id";
        var rows = await connection.QueryAsync<dynamic>(BuildSqlWithSchema(sql), new { id });
        var userProfiles = MapUserProfiles(rows);
        return userProfiles.FirstOrDefault();
    }

    public async Task<IEnumerable<UserProfile>> GetUsersAsync(string[] ids)
    {
        using var connection = new NpgsqlConnection(_appSettings.IdentityDbConnection);
        connection.Open();
        var sql = @"SELECT
                u.""Id"", u.""UserName"", u.""Email"", u.""PhoneNumber"", u.""CreatedAt"", u.""UpdatedAt"",
                user_claim.""ClaimType"", user_claim.""ClaimValue""
                FROM ""users"" u
                LEFT JOIN ""user_claims"" user_claim ON user_claim.""UserId"" = u.""Id""
                WHERE u.""Id"" = ANY(@ids)";
        var rows = await connection.QueryAsync<dynamic>(BuildSqlWithSchema(sql), new { ids });
        return MapUserProfiles(rows);
    }

    private IEnumerable<UserProfile> MapUserProfiles(IEnumerable<dynamic> rows)
    {
        var groupedRows = rows.GroupBy(x => x.Id);
        var userProfiles = new List<UserProfile>();
        foreach (var group in groupedRows)
        {
            var row = group.First();
            var userProfile = new UserProfile
            {
                Id = row.Id,
                UserName = row.UserName,
                Email = row.Email,
                PhoneNumber = row.PhoneNumber,
                CreatedAt = row.CreatedAt,
                UpdatedAt = row.UpdatedAt
            };
            var claims = group.Select(x => new { Type = x.ClaimType, Value = x.ClaimValue });
            foreach (var claim in claims)
            {
                switch (claim.Type)
                {
                    case JwtClaimTypes.GivenName:
                        userProfile.GivenName = claim.Value;
                        break;
                    case JwtClaimTypes.FamilyName:
                        userProfile.FamilyName = claim.Value;
                        break;
                    case JwtClaimTypes.Name:
                        userProfile.DisplayName = claim.Value;
                        break;
                    case JwtClaimTypes.Picture:
                        userProfile.PictureId = claim.Value;
                        break;
                    case JwtClaimTypes.BirthDate:
                        userProfile.BirthDate = claim.Value;
                        break;
                    case JwtClaimTypes.ZoneInfo:
                        userProfile.ZoneInfo = claim.Value;
                        break;
                    case JwtClaimTypes.Locale:
                        userProfile.Locale = claim.Value;
                        break;
                    case "bio":
                        userProfile.Bio = claim.Value;
                        break;
                }
            }
            userProfiles.Add(userProfile);
        }
        return userProfiles;
    }
}
