using System.Security.Cryptography;
using System.Text;

namespace Turnero.Web.Controllers;

[AllowAnonymous]
[Route("setup")]
public sealed class SetupController(
    IFirebaseAuthService firebaseAuthService,
    IConfiguration configuration,
    InitialSetupState setupState,
    ILogger<SetupController> logger) : Controller
{
    private static readonly SelectListItem[] Roles =
    [
        new("Administrador (Admin)", RolesConstants.Admin),
        new("Médico (Medico)", RolesConstants.Medico),
        new("Recepción (Ingreso)", RolesConstants.Ingreso)
    ];

    [HttpGet]
    public IActionResult Index()
    {
        SetNoStore();
        if (!setupState.IsRequired)
        {
            return RedirectToPage("/Account/Login", new { area = "Identity" });
        }

        var model = PrepareModel();
        if (!HasValidConfiguredToken())
        {
            model.SetupError = "Configure InitialSetup__Token con al menos 32 caracteres aleatorios antes de completar el setup.";
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("InitialSetup")]
    public async Task<IActionResult> Index(InitialSetupViewModel model)
    {
        SetNoStore();
        await setupState.Gate.WaitAsync(HttpContext.RequestAborted);
        try
        {
            if (!setupState.IsRequired)
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            model = PrepareModel(model);
            if (!HasValidConfiguredToken())
            {
                model.SetupError = "Configure InitialSetup__Token con al menos 32 caracteres aleatorios antes de completar el setup.";
                return Render(model);
            }

            if (!IsSetupTokenValid(model.SetupToken))
            {
                ModelState.AddModelError(nameof(model.SetupToken), "El token de configuración no es válido.");
            }

            if (!ModelState.IsValid)
            {
                return Render(model);
            }

            var adminUsers = model.Users.Where(user => user.Role == RolesConstants.Admin).ToList();
            foreach (var user in adminUsers)
            {
                var result = await RegisterUserAsync(user);
                if (result.Succeeded)
                {
                    model.CreatedUsers.Add(ToResult(user));
                    setupState.Complete();
                }
                else
                {
                    model.UserErrors.Add(FormatRegistrationError(user, result.Errors));
                }
            }

            if (setupState.IsRequired)
            {
                model.ResultMessage = "No se pudo crear un administrador. Revise los errores y vuelva a intentarlo.";
                return Render(model);
            }

            foreach (var user in model.Users.Where(user => user.Role != RolesConstants.Admin))
            {
                var result = await RegisterUserAsync(user);
                if (result.Succeeded)
                {
                    model.CreatedUsers.Add(ToResult(user));
                }
                else
                {
                    model.UserErrors.Add(FormatRegistrationError(user, result.Errors));
                }
            }

            model.SetupComplete = true;
            model.ResultMessage = model.UserErrors.Count == 0
                ? "La instancia quedó configurada. Ya puede iniciar sesión."
                : "La instancia quedó configurada, pero algunas cuentas no se crearon. Inicie sesión como administrador para completarlas.";

            return Render(model);
        }
        finally
        {
            setupState.Gate.Release();
        }
    }

    private async Task<RegistrationResultDto> RegisterUserAsync(InitialSetupUserViewModel user)
    {
        var result = await firebaseAuthService.RegisterUserAsync(new UserFirebaseDTO
        {
            Name = user.Name.Trim(),
            Email = user.Email.Trim(),
            Password = user.Password,
            Role = user.Role,
            SendEmailVerification = false
        });

        if (!result.Succeeded)
        {
            logger.LogWarning("Initial setup could not register a user with role {Role}: {Errors}",
                user.Role, string.Join("; ", result.Errors.Select(error => error.Code)));
        }

        return result;
    }

    private InitialSetupViewModel PrepareModel(InitialSetupViewModel? model = null)
    {
        model ??= new InitialSetupViewModel();
        model.RoleOptions = [.. Roles.Select(role => new SelectListItem(role.Text, role.Value))];
        return model;
    }

    private IActionResult Render(InitialSetupViewModel model)
    {
        model.Users ??= [];
        model.SetupToken = string.Empty;
        ModelState.Remove(nameof(model.SetupToken));
        for (var index = 0; index < model.Users.Count; index++)
        {
            model.Users[index].Password = string.Empty;
            model.Users[index].ConfirmPassword = string.Empty;
            ModelState.Remove($"Users[{index}].Password");
            ModelState.Remove($"Users[{index}].ConfirmPassword");
        }

        return View("Index", model);
    }

    private bool HasValidConfiguredToken()
    {
        var token = configuration["InitialSetup:Token"];
        return !string.IsNullOrWhiteSpace(token) && Encoding.UTF8.GetByteCount(token) >= 32;
    }

    private bool IsSetupTokenValid(string? providedToken)
    {
        var configuredToken = configuration["InitialSetup:Token"];
        if (string.IsNullOrEmpty(configuredToken) || string.IsNullOrEmpty(providedToken))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(configuredToken),
            Encoding.UTF8.GetBytes(providedToken));
    }

    private static InitialSetupUserResult ToResult(InitialSetupUserViewModel user) =>
        new(user.Name.Trim(), user.Email.Trim(), user.Role);

    private static string FormatRegistrationError(
        InitialSetupUserViewModel user,
        IEnumerable<RegistrationErrorDto> errors)
    {
        var description = string.Join(" ", errors.Select(error => error.Description).Where(value => !string.IsNullOrWhiteSpace(value)));
        return string.IsNullOrWhiteSpace(description)
            ? $"No se pudo crear la cuenta {user.Email}."
            : $"No se pudo crear la cuenta {user.Email}: {description}";
    }

    private void SetNoStore() =>
        Response.Headers.CacheControl = "no-store";
}
