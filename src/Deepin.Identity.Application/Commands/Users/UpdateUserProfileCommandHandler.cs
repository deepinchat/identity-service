using Deepin.Domain;
using Deepin.Domain.Exceptions;
using Deepin.Identity.Application.Models.Users;
using Deepin.Identity.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Deepin.Identity.Application.Commands.Users;

public class UpdateUserProfileCommandHandler(IUserContext userContext, IdentityContext db) : IRequestHandler<UpdateUserProfileCommand, UserProfile>
{
    private readonly IUserContext _userContext = userContext;
    private readonly IdentityContext _db = db;
    public async Task<UserProfile> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _db.Users.FindAsync(_userContext.UserId, cancellationToken);
        if (user == null)
        {
            throw new DomainException($"User with id {_userContext.UserId} not found");
        }
        var claims = await _db.UserClaims.Where(x => x.UserId == _userContext.UserId).ToListAsync(cancellationToken);
        _db.UserClaims.RemoveRange(claims);
        var newClaims = request.ToClaims();
        _db.UserClaims.AddRange(newClaims.Select(x => new IdentityUserClaim<string>
        {
            UserId = _userContext.UserId,
            ClaimType = x.Type,
            ClaimValue = x.Value
        }));
        user.UpdatedAt = DateTime.UtcNow;
        _db.Users.Update(user);
        await _db.SaveChangesAsync(cancellationToken);
        return new UserProfile
        {
            Id = user.Id,
            UserName = user.UserName,
            CreatedAt = user.CreatedAt,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            UpdatedAt = user.UpdatedAt,

            GivenName = request.GivenName,
            FamilyName = request.FamilyName,
            DisplayName = request.DisplayName,
            PictureId = request.PictureId,
            BirthDate = request.BirthDate,
            ZoneInfo = request.ZoneInfo,
            Locale = request.Locale,
            Bio = request.Bio,
        };
    }
}
