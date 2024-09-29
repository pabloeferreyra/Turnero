using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Turnero.Services;
namespace Turnero.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel(SignInManager<IdentityUser> signInManager,
        ILogger<LoginModel> logger,
        UserManager<IdentityUser> userManager, IFirebaseService firebaseService) : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager = userManager;
        private readonly SignInManager<IdentityUser> _signInManager = signInManager;
        private readonly ILogger<LoginModel> _logger = logger;
        private readonly IFirebaseService _firebaseService = firebaseService;

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

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

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
                UserLoginRequestDTO userLogin = new() { Email = $"{Input.User}@consultorios.com", Password = Input.Password };
                var firebaseRes = await _firebaseService.LoginAsync(userLogin);
                if (firebaseRes.LocalId != null)
                {
                    _logger.LogInformation("User logged in.");
                    var user = await _userManager.FindByIdAsync(firebaseRes.LocalId);
                    
                    await _signInManager.SignInAsync(user, Input.RememberMe);
                    
                    if (await _userManager.IsInRoleAsync(user, "Admin"))
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
}
