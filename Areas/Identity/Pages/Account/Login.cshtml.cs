using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TurnovoFarmer.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<IdentityUser> signInManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Моля, въведете имейл адрес.")]
            [EmailAddress(ErrorMessage = "Невалиден имейл адрес.")]
            [Display(Name = "Имейл")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Моля, въведете парола.")]
            [DataType(DataType.Password)]
            [Display(Name = "Парола")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Запомни ме")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl;
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }

            ReturnUrl = returnUrl;
            ModelState.AddModelError(string.Empty, "Невалиден имейл или парола.");
            return Page();
        }
    }
}
