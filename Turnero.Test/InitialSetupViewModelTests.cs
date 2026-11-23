using System.ComponentModel.DataAnnotations;
using Turnero.Web.Models;

public class InitialSetupViewModelTests
{
    [Fact]
    public void Validate_RequiresAnAdminUser()
    {
        var model = new InitialSetupViewModel
        {
            SetupToken = new string('x', 32),
            Users = [CreateUser("person@example.com", RolesConstants.Medico)]
        };

        var errors = Validate(model);

        Assert.Contains(errors, error => error.ErrorMessage!.Contains("Admin"));
    }

    [Fact]
    public void Validate_RejectsDuplicateEmailsIgnoringCase()
    {
        var model = new InitialSetupViewModel
        {
            SetupToken = new string('x', 32),
            Users =
            [
                CreateUser("admin@example.com", RolesConstants.Admin),
                CreateUser("ADMIN@example.com", RolesConstants.Medico)
            ]
        };

        var errors = Validate(model);

        Assert.Contains(errors, error => error.ErrorMessage!.Contains("repetido"));
    }

    [Fact]
    public void Validate_RejectsRolesOutsideTheApplicationRoles()
    {
        var model = new InitialSetupViewModel
        {
            SetupToken = new string('x', 32),
            Users = [CreateUser("admin@example.com", "SuperAdmin")]
        };

        var errors = Validate(model);

        Assert.Contains(errors, error => error.ErrorMessage!.Contains("rol válido"));
    }

    private static List<ValidationResult> Validate(InitialSetupViewModel model)
    {
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(model, new ValidationContext(model), errors, validateAllProperties: true);
        return errors;
    }

    private static InitialSetupUserViewModel CreateUser(string email, string role) => new()
    {
        Name = "User",
        Email = email,
        Password = "password",
        ConfirmPassword = "password",
        Role = role
    };
}
