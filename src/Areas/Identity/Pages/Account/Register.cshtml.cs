// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using src.Areas.Identity.Data;
using src.Helpers;

namespace src.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<srcUser> _signInManager;
        private readonly UserManager<srcUser> _userManager;
        private readonly IUserStore<srcUser> _userStore;
        private readonly IUserEmailStore<srcUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly GooglereCAPTCHAService _GoogleReCHAPTCHAService;

        public RegisterModel(
            UserManager<srcUser> userManager,
            IUserStore<srcUser> userStore,
            SignInManager<srcUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            GooglereCAPTCHAService googlereCAPTCHAService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _GoogleReCHAPTCHAService = googlereCAPTCHAService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [StringLength(50)]
            [DataType(DataType.Text)]
            [Display(Name = "Voornaam")]
            public string FirstName { get; set; }

            [Required]
            [StringLength(50)]
            [DataType(DataType.Text)]
            [Display(Name = "Achternaam")]
            public string LastName { get; set; }

            [StringLength(50)]
            [Display(Name = "IBAN")]
            public string IBAN { get; set; }

            [Display(Name = "BSN")]
            public string BSN { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [SixteenAndOlder]
            [Display(Name = "Geboortedatum")]
            public DateTime Age { get; set; }

            [Display(Name = "Bent u een ouder?")]
            public bool Parent { get; set; }
            public string Token {get;set;}
        }


        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            
            //Google ReCAPTCHA
            var _GoogleReCHAPTCHA= _GoogleReCHAPTCHAService.VertifyResponse(Input.Token);

            //Hier wordt gekeken of het resultaat is gelukt. en of de score van de ReCAPTCHA boven de 0.5 is.
            if(!_GoogleReCHAPTCHA.Result.success && _GoogleReCHAPTCHA.Result.score>=0.5){
                    ModelState.AddModelError(string.Empty, "ReCAPTCHA gefaald, probeer opnieuw.");
            }else if (ModelState.IsValid)
            {
                srcUser user;
                //hier wordt gekeken of de gene die het veld invoerd een parent is
                if(Input.Parent){
                user = new srcUser
                    {
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        Age = Input.Age,
                        Email = Input.Email,
                        IBAN = Input.IBAN,
                        BSN = Input.BSN
                    };
                }else{
                    user = new srcUser
                    {
                        FirstName = Input.FirstName,
                        LastName = Input.LastName,
                        Age = Input.Age
                    };
                }

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                //await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    if(await SetRoleAsync(user)){
                        _logger.LogInformation("Role has been added to the User.");
                    }else{
                         _logger.LogInformation("Adding role to user failed.");
                    }

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
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
        public async Task<bool> SetRoleAsync(srcUser user){
            if(Input.Parent)
            {
                await _userManager.AddToRoleAsync(user, "Ouder");
                return await _userManager.IsInRoleAsync(user, "Ouder");
            }
            else
            {
                await _userManager.AddToRoleAsync(user, "Client");
                return await _userManager.IsInRoleAsync(user, "Client");
            }
        }

        private IUserEmailStore<srcUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<srcUser>)_userStore;
        }
    }
}
