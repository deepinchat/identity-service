using Microsoft.AspNetCore.Identity;

namespace Deepin.Identity.Domain.Entities;
public class User : IdentityUser
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}