namespace Turnero.DAL.Models;

public class UserFirebaseDTO
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string? Role { get; set; }

}

public class UserLoginRequestDTO
{
    public string Email { get; set; }
    public string Password { get; set; }
}
public class UserResetPasswordDTO
{
    public string Email { get; set; }
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
}