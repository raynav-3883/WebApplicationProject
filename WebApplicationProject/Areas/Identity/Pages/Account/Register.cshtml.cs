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
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using WebApplicationProject.Areas.Identity.Data;
using Microsoft.AspNetCore.Http;

namespace WebApplicationProject.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ProjectUser> _signInManager;
        private readonly UserManager<ProjectUser> _userManager;
        private readonly IUserStore<ProjectUser> _userStore;
        private readonly IUserEmailStore<ProjectUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ProjectUser> userManager,
            IUserStore<ProjectUser> userStore,
            SignInManager<ProjectUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

      
        [BindProperty]
        public InputModel Input { get; set; }

      
        public string ReturnUrl { get; set; }

       
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

       
        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            [Display(Name ="First Name")]
            public string FirstName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }


            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Father First Name")]
            public string FatherFirstName { get; set; }

            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Father Last Name")]
            public string FatherLastName { get; set; }

            
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

           
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            
            [Required(ErrorMessage = "The {0} field is required.")]
            [Display(Name = "Date of Birth")]
            [DataType(DataType.Date)]
            [MinimumAge(27, ErrorMessage = "You must be at least 27 years old.")]
            public DateTime DateOfBirth { get; set; }

            [Required]
            [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile number must be 10 digits.")]
            [Display(Name = "Mobile Number")]
            public string MobileNumber { get; set; }

            [Required(ErrorMessage = "Profile picture is required.")]
            [Display(Name = "Profile Picture")]
            public IFormFile ProfilePicture { get; set; }


        }

        public class MinimumAgeAttribute(int minimumAge) : ValidationAttribute
        {
            private readonly int _minimumAge = minimumAge;

            protected override ValidationResult IsValid(object value, ValidationContext validationContext)
            {
                if (value is DateTime dateOfBirth)
                {
                    if (DateTime.Today.AddYears(-_minimumAge) >= dateOfBirth)
                    {
                        return ValidationResult.Success;
                    }
                    return new ValidationResult(ErrorMessage);
                }

                return new ValidationResult("Invalid date format.");
            }
        }



        public async Task OnGetAsync(string returnUrl = null)
        {
            if (User.Identity.IsAuthenticated)
            {
                Response.Redirect("/");
            }
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                user.FirstName = Input.FirstName;
                user.LastName = Input.LastName;
                user.DateOfBirth = Input.DateOfBirth;
                user.MobileNumber = Input.MobileNumber;
                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (Input.ProfilePicture != null && Input.ProfilePicture.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        Input.ProfilePicture.CopyTo(ms);
                        var pictureArray = ms.ToArray();

                        
                        user.ProfilePictureData = pictureArray; 
                    }
                }

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);

                        
                        return RedirectToPage("/Acknowledgment", new
                        {
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Email = user.Email,
                           
                        });
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

           
            return Page();
        }


        private ProjectUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ProjectUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ProjectUser)}'. " +
                    $"Ensure that '{nameof(ProjectUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ProjectUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ProjectUser>)_userStore;
        }
    }
}
