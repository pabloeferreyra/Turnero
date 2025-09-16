namespace Turnero.DAL.Models;

public class EditUserViewModel
{
    public EditUserViewModel()
    {
        Id = string.Empty;
        UserName = string.Empty;
        Email = string.Empty;
        Claims = [];
        Roles = [];
    }

    public string Id { get; set; }

    [Required]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    public List<string> Claims { get; set; }
    public IList<string> Roles { get; set; }
}
