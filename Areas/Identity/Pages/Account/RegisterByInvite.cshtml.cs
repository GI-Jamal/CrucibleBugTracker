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
using CrucibleBugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using CrucibleBugTracker.Data;
using CrucibleBugTracker.Enums;
using CrucibleBugTracker.Services.Interfaces;

namespace CrucibleBugTracker.Areas.Identity.Pages.Account
{
    public class RegisterByInviteModel : PageModel
    {
        private readonly SignInManager<BTUser> _signInManager;
        private readonly UserManager<BTUser> _userManager;
        private readonly IUserStore<BTUser> _userStore;
        private readonly IUserEmailStore<BTUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IBTInviteService _inviteService;
        private readonly IBTProjectService _projectService;


        public RegisterByInviteModel(
            UserManager<BTUser> userManager,
            IUserStore<BTUser> userStore,
            SignInManager<BTUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            IBTInviteService inviteService,
            IBTProjectService projectService)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _inviteService = inviteService;
            _projectService = projectService;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; } = new();

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
            /// 
            /// </summary>
            [Required]
            [Display(Name = "First Name")]
            public string FirstName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [Required]
            [Display(Name = "Last Name")]
            public string LastName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [Display(Name = "Company Name")]
            public string CompanyName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            [Required]
            public int CompanyId { get; set; }


            /// <summary>
            /// 
            /// </summary>
            [Required]
            public string Token { get; set; }

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
        }


        public async Task OnGetAsync(string token, int companyId, int id, string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            Invite invite = await _inviteService.GetInviteAsync(id, companyId);

            Input = new InputModel
            {
                Email = invite.InviteeEmail,
                FirstName = invite.InviteeFirstName,
                LastName = invite.InviteeLastName,
                CompanyName = invite.Company.Name,
                CompanyId = invite.CompanyId,
                Token = token
            };
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            Invite invite = await _inviteService.GetInviteAsync(Guid.Parse(Input.Token), Input.Email, Input.CompanyId);

            if (invite is null || await _inviteService.ValidateInviteAsync(invite.CompanyToken) == false)
            {
                ModelState.AddModelError(string.Empty, "Invalid invite. Please contact the company owner for a new invite");
                return Page();
            }

            if (ModelState.IsValid)
            {
                BTUser user = new()
                {
                    FirstName = Input.FirstName,
                    LastName = Input.LastName,
                    Email = invite.InviteeEmail,
                    CompanyId = invite.CompanyId
                };

                await _userStore.SetUserNameAsync(user, invite.InviteeEmail, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, invite.InviteeEmail, CancellationToken.None);
                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    if (invite.ProjectId is not null or 0)
                    {
                        await _projectService.AddMemberToProjectAsync(user, invite.ProjectId.Value, user.CompanyId);
                    }

                    await _userManager.AddToRoleAsync(user, nameof(BTRoles.Submitter));

                    _logger.LogInformation("User created a new account with password.");

                    var userId = await _userManager.GetUserIdAsync(user);

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId, code, returnUrl },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    await _inviteService.AcceptInviteAsync(invite.CompanyToken, userId, invite.CompanyId);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl });
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

        private IUserEmailStore<BTUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<BTUser>)_userStore;
        }
    }
}
