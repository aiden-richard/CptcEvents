// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using CptcEvents.Models;
using CptcEvents.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace CptcEvents.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IInstructorCodeService _instructorCodeService;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IInstructorCodeService instructorCodeService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _instructorCodeService = instructorCodeService;
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

            [Required]
            [Display(Name = "Username")]
            [StringLength(50)]
            public string Username { get; set; }

            /// <summary>
            /// Gets or sets the first name of the person.
            /// </summary>
            [Required]
            [StringLength(50)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the person.
            /// </summary>
            [Required]
            [StringLength(50)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            /// <summary>
            /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            /// directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            /// This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            /// directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            /// <summary>
            /// Gets or sets the instructor code.
            /// </summary>
            [Display(Name = "Instructor Code")]
            [StringLength(8, MinimumLength = 8, ErrorMessage = "The instructor code must be exactly 8 characters long.")]
            public string InstructorCode { get; set; }
        }


        public async Task OnGetAsync(string returnUrl = null, string instructorCode = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            Input = new InputModel { InstructorCode = instructorCode };
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                bool isValidInstructorCode = !String.IsNullOrEmpty(Input.InstructorCode) ? await _instructorCodeService.ValidateCodeAsync(Input.InstructorCode, Input.Email) : false;
                if (!isValidInstructorCode && !string.IsNullOrWhiteSpace(Input.InstructorCode))
                {
                    ModelState.AddModelError("Input.InstructorCode", "Invalid instructor code.");
                    return Page();
                }

                var user = CreateUser();

                // Set additional properties
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;

                await _userStore.SetUserNameAsync(user, Input.Username, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    string role = isValidInstructorCode && !string.IsNullOrWhiteSpace(Input.InstructorCode) ? "Staff" : "Student";
                    await _userManager.AddToRoleAsync(user, role);

                    await _instructorCodeService.MarkCodeAsUsedAsync(Input.InstructorCode, user.Id);

                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    string subject = "Confirm Your CPTC Events Email";
                    string htmlMessage = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                                <h2 style='color: #502a7f;'>Welcome to CPTC Events, {HtmlEncoder.Default.Encode(user.FirstName)}!</h2>
                                <p>Thank you for registering with CPTC Events. We're excited to have you join our community!</p>
                                
                                <div style='background-color: #f4f4f4; border-left: 4px solid #502a7f; padding: 15px; margin: 20px 0;'>
                                    <p style='margin: 0;'><strong>Account Details:</strong></p>
                                    <p style='margin: 5px 0;'><strong>Username:</strong> {HtmlEncoder.Default.Encode(user.UserName)}</p>
                                    <p style='margin: 5px 0;'><strong>Email:</strong> {HtmlEncoder.Default.Encode(user.Email)}</p>
                                    <p style='margin: 5px 0;'><strong>Role:</strong> {HtmlEncoder.Default.Encode(role)}</p>
                                </div>
                                
                                <p>To complete your registration and activate your account, please confirm your email address by clicking the button below:</p>
                                
                                <p style='margin: 30px 0; text-align: center;'>
                                    <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' 
                                       style='background-color: #502a7f; color: white; padding: 12px 24px; text-decoration: none; border-radius: 4px; display: inline-block;'>
                                        Confirm Email Address
                                    </a>
                                </p>
                                
                                <p style='font-size: 12px; color: #666;'>
                                    Or copy and paste this link into your browser:<br>
                                    <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>{HtmlEncoder.Default.Encode(callbackUrl)}</a>
                                </p>
                                
                                <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                                
                                <h3 style='color: #502a7f;'>What's Next?</h3>
                                <ul>
                                    <li>Browse and join groups that interest you</li>
                                    <li>Stay updated with upcoming CPTC events</li>
                                    <li>Connect with fellow students and staff</li>
                                </ul>
                                
                                <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                                
                                <p style='font-size: 12px; color: #666;'>
                                    If you did not create this account, please disregard this email.
                                </p>
                            </div>
                        </body>
                        </html>";

                    await _emailSender.SendEmailAsync(Input.Email, subject, htmlMessage);

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

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}
