using System.Security.Claims;
using Deepin.Identity.Application.Models.Users;
using IdentityModel;
using MediatR;

namespace Deepin.Identity.Application.Commands.Users;

public class UpdateUserProfileCommand : IRequest<UserProfile>
{
    public string Id { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public string DisplayName { get; set; }
    public string PictureId { get; set; }
    public string BirthDate { get; set; }
    public string ZoneInfo { get; set; }
    public string Locale { get; set; }
    public string Bio { get; set; }
    public IEnumerable<Claim> ToClaims()
    {
        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(GivenName))
        {
            claims.Add(new Claim(JwtClaimTypes.GivenName, GivenName));
        }
        if (!string.IsNullOrWhiteSpace(FamilyName))
        {
            claims.Add(new Claim(JwtClaimTypes.FamilyName, FamilyName));
        }
        if (!string.IsNullOrWhiteSpace(DisplayName))
        {
            claims.Add(new Claim(JwtClaimTypes.Name, DisplayName));
        }
        if (!string.IsNullOrWhiteSpace(PictureId))
        {
            claims.Add(new Claim(JwtClaimTypes.Picture, PictureId));
        }
        if (!string.IsNullOrWhiteSpace(BirthDate))
        {
            claims.Add(new Claim(JwtClaimTypes.BirthDate, BirthDate));
        }
        if (!string.IsNullOrWhiteSpace(ZoneInfo))
        {
            claims.Add(new Claim(JwtClaimTypes.ZoneInfo, ZoneInfo));
        }
        if (!string.IsNullOrWhiteSpace(Locale))
        {
            claims.Add(new Claim(JwtClaimTypes.Locale, Locale));
        }
        if (!string.IsNullOrWhiteSpace(Bio))
        {
            claims.Add(new Claim("bio", Bio));
        }
        return claims;
    }
}