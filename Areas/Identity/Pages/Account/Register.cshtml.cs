namespace Turnero.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "Admin")]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IFirebaseService _firebaseService;
        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IFirebaseService firebaseService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _firebaseService = firebaseService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [Display(Name = "User")]
            public string UserName { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = [.. (await _signInManager.GetExternalAuthenticationSchemesAsync())];
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            ExternalLogins = [.. (await _signInManager.GetExternalAuthenticationSchemesAsync())];
            if (ModelState.IsValid)
            {
                var result = await _firebaseService.RegisterAdminAsync(new UserFirebaseDTO { Email = Input.Email, Name = Input.UserName, Password = Input.Password });
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");



                    if (_signInManager.IsSignedIn(User) && User.IsInRole(RolesConstants.Admin))
                    {
                        return RedirectToAction("ListUsers", "Administration");
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
