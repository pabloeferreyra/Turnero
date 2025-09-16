namespace Turnero.DAL.Models;

public class UserClaimsViewModel
{
    public UserClaimsViewModel()
    {
        UserId = string.Empty;
        Claims = [];
    }

    public string UserId { get; set; }
    public List<UserClaim> Claims { get; set; }
}
