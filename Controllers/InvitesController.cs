using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CrucibleBugTracker.Data;
using CrucibleBugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using CrucibleBugTracker.Enums;
using CrucibleBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using CrucibleBugTracker.Extensions;
using System.Net.Mail;

namespace CrucibleBugTracker.Controllers
{
    [Authorize(Roles = nameof(BTRoles.Admin))]
    public class InvitesController : Controller
    {
        private readonly IBTInviteService _inviteService;
        private readonly IBTProjectService _projectService;
        private readonly IEmailSender _emailService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IDataProtector _protector;
        private readonly string _protectorPurpose;

        public InvitesController(IBTInviteService inviteService,
                                 IBTProjectService projectService,
                                 IEmailSender emailSender,
                                 UserManager<BTUser> userManager,
                                 IConfiguration configuration,
                                 IDataProtectionProvider protectionProvider)
        {
            _inviteService = inviteService;
            _projectService = projectService;
            _emailService = emailSender;
            _userManager = userManager;
            _protectorPurpose = configuration.GetValue<string>("ProtectorString") ?? Environment.GetEnvironmentVariable("ProtectorString")!;
            _protector = protectionProvider.CreateProtector(_protectorPurpose);
        }

        // GET: Invites/Create
        public async Task<IActionResult> Create()
        {   
            List<Project> companyProjects = await _projectService.GetAllProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());

            ViewData["ProjectId"] = new SelectList(companyProjects, "Id", "Name");
            
            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("InviteeEmail,InviteeFirstName,InviteeLastName,Message,ProjectId")] Invite invite)
        {           
            int companyId = User.Identity!.GetCompanyId();

            ModelState.Remove(nameof(Invite.InvitorId));

            if (ModelState.IsValid)
            {
                try
                {
                    Guid guid = Guid.NewGuid();

                    invite.CompanyToken = guid;
                    invite.CompanyId = companyId;
                    invite.InviteDate = DateTime.UtcNow;
                    invite.InvitorId = _userManager.GetUserId(User);
                    invite.IsValid = true;

                    await _inviteService.AddNewInviteAsync(invite);

                    string token = _protector.Protect(guid.ToString());
                    string email = _protector.Protect(invite.InviteeEmail!);
                    string company = _protector.Protect(companyId.ToString());

                    string? callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email, company }, Request.Scheme);

                    string body = $@"<h4>You've been invited to join the Crucible Bug Tracker!</h4><br />
                                        {invite.Message}<br /><br />
                                        <a href=""{callbackUrl}"">Click here</a> to join our team.";

                    string subject = "You've been invited to join the Crucible Bug Tracker!";

                    await _emailService.SendEmailAsync(invite.InviteeEmail!, subject, body);

                    TempData["SuccessMessage"] = $"Invite sent to {invite.InviteeEmail} successfully.";

                    return RedirectToAction("Index", "Home");
                }
                catch (Exception)
                {
                    throw;
                }
            }

            List<Project> companyProjects = await _projectService.GetAllProjectsByCompanyIdAsync(User.Identity!.GetCompanyId());
            ViewData["ProjectId"] = new SelectList(companyProjects, "Id", "Name", invite.ProjectId);
            return View(invite);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ProcessInvite(string? token, string? email, string? company)
        {
            if (string.IsNullOrEmpty(token)|| string.IsNullOrEmpty(email) || string.IsNullOrEmpty(company))
            {
                return NotFound();
            }

            int companyId = int.Parse(_protector.Unprotect(company));
            string inviteeEmail = _protector.Unprotect(email);
            Guid companyToken = Guid.Parse(_protector.Unprotect(token));

            try
            {
                Invite? invite = await _inviteService.GetInviteAsync(companyToken, inviteeEmail, companyId);

                if (invite is null)
                {
                    return NotFound();
                }

                return View(invite);
            }
            catch (Exception)
            {
                throw;
            }
        }   
    }
}
