using Microsoft.AspNetCore.Identity.UI.Services;

namespace Turnero.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ForgotPasswordModel(UserManager<IdentityUser> userManager, IEmailSender emailSender, IFirebaseService firebaseService) : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(Input.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return RedirectToPage("./ForgotPasswordConfirmation");
                }

                await firebaseService.SendPasswordResetLinkAsync(Input.Email);
                
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            return Page();
        }
    }
}
