namespace Turnero.Models;

public class UserRegisterRequestDTO
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