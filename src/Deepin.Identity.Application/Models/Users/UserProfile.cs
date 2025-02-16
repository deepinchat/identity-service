namespace Deepin.Identity.Application.Models.Users;

public class UserProfile
{
    public string Id { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string GivenName { get; set; }
    public string FamilyName { get; set; }
    public string DisplayName { get; set; }
    public string PictureId { get; set; }
    public string BirthDate { get; set; }
    public string ZoneInfo { get; set; }
    public string Locale { get; set; }
    public string Bio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
