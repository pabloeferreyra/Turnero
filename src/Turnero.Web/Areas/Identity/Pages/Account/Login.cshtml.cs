namespace Turnero.Web.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel(SignInManager<IdentityUser> signInManager,
    ILogger<LoginModel> logger,
    UserManager<IdentityUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IFirebaseAuthService firebaseService) : PageModel
{
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;
    private readonly SignInManager<IdentityUser> _signInManager = signInManager;
    private readonly ILogger<LoginModel> _logger = logger;
    private readonly IFirebaseAuthService _firebaseService = firebaseService;

    [BindProperty]
    public InputModel Input { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public string ReturnUrl { get; set; }

    [TempData]
    public string ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        public string User { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            ModelState.AddModelError(string.Empty, ErrorMessage);
        }

        returnUrl ??= Url.Content("~/");

        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ExternalLogins = [.. await _signInManager.GetExternalAuthenticationSchemesAsync()];

        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/Turns");

        if (ModelState.IsValid)
        {
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, set lockoutOnFailure: true
            Input.RememberMe = true;
            UserLoginRequestDTO userLogin = new()
            {
                Email = Input.User.Contains('@') ? Input.User : $"{Input.User}@consultorios.com",
                Password = Input.Password
            };
            var firebaseRes = await _firebaseService.LoginAsync(userLogin);
            if (!string.IsNullOrWhiteSpace(firebaseRes.LocalId) && !string.IsNullOrWhiteSpace(firebaseRes.IdToken))
            {
                _logger.LogInformation("User logged in.");
                var user = await _userManager.FindByIdAsync(firebaseRes.LocalId);

                // Los claims vienen decodificados en el DTO (el Web no usa el SDK de Firebase)
                var role = firebaseRes.Claims.TryGetValue("role", out var roleClaim)
                    ? roleClaim?.ToString()
                    : null;
                var hasFirebaseRole = !string.IsNullOrWhiteSpace(role);

                if (user == null)
                {
                    role ??= RolesConstants.Medico;
                    user = new IdentityUser
                    {
                        Id = firebaseRes.LocalId,
                        UserName = firebaseRes.DisplayName ?? userLogin.Email,
                        Email = firebaseRes.Email ?? userLogin.Email
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        foreach (var error in createResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return Page();
                    }
                }

                if (string.IsNullOrWhiteSpace(role))
                {
                    role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                    role ??= RolesConstants.Medico;
                }

                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                if (!await _userManager.IsInRoleAsync(user, role))
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, role);
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError(string.Empty, error.Description);
                        }
                        return Page();
                    }
                }

                if (!hasFirebaseRole)
                {
                    await _firebaseService.SetRoleAsync(user.Id, role);
                }

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.Id),
                    new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
                    new(ClaimTypes.Email, user.Email ?? string.Empty)
                };
                if (!string.IsNullOrWhiteSpace(role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var principal = new ClaimsPrincipal(new ClaimsIdentity(
                    claims,
                    IdentityConstants.ApplicationScheme,
                    ClaimTypes.Name,
                    ClaimTypes.Role));
                await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, principal,
                    new AuthenticationProperties { IsPersistent = Input.RememberMe });

                if (string.Equals(role, RolesConstants.Admin, StringComparison.OrdinalIgnoreCase))
                {
                    returnUrl = Url.Content("~/");
                }
                return LocalRedirect(returnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Intento de ingreso invalido.");
                return Page();
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }
}
