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
using CrucibleBugTracker.Extensions;
using CrucibleBugTracker.Services.Interfaces;
using CrucibleBugTracker.Enums;
using Microsoft.AspNetCore.Identity;
using CrucibleBugTracker.Models.ViewModels;
using Newtonsoft.Json;

namespace CrucibleBugTracker.Controllers
{
    [Authorize]
    public class CompanyController : Controller
    {
        private readonly IBTCompanyService _companyService;
        private readonly IBTRoleService _roleService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTTicketHistoryService _ticketHistoryService;

        public CompanyController(IBTCompanyService companyService, IBTRoleService roleService, UserManager<BTUser> userManager, IBTProjectService projectService, IBTTicketService ticketService, IBTTicketHistoryService ticketHistoryService)
        {
            _companyService = companyService;
            _roleService = roleService;
            _userManager = userManager;
            _projectService = projectService;
            _ticketService = ticketService;
            _ticketHistoryService = ticketHistoryService;
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();
            Company? company = await _companyService.GetCompanyInfoAsync(companyId);

            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [HttpGet]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> ManageUserRoles()
        {
            List<BTUser> members = await _companyService.GetCompanyMembersAsync(User.Identity!.GetCompanyId());
            List<IdentityRole> roles = await _roleService.GetRolesAsync();
            List<ManageUserRolesViewModel> model = new();

            foreach (BTUser member in members)
            {
                if (member.Id != _userManager.GetUserId(User) && !await _roleService.IsUserInRole(member, nameof(BTRoles.DemoUser)))
                {
                    IEnumerable<string> userRoles = await _roleService.GetUserRolesAsync(member);

                    ManageUserRolesViewModel viewModel = new()
                    {
                        Roles = new SelectList(roles, "Name", "Name", userRoles.FirstOrDefault()),
                        User = member,
                        SelectedRole = userRoles.FirstOrDefault()
                    };

                    model.Add(viewModel);
                }
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> ManageUserRoles(string? id, string? newRole)
        {
            string? selectedRole = newRole;
            string? userId = id;
            int companyId = User.Identity!.GetCompanyId();
            string? currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(selectedRole))
            {
                return Json(new { success = false });
            }

            BTUser? user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return Json(new { success = false });
            }

            List<BTUser> admins = await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Admin), User.Identity!.GetCompanyId());

            
            IEnumerable<string> currentRoles = await _roleService.GetUserRolesAsync(user);

            if (admins.Count <= 1 && currentRoles.Contains(nameof(BTRoles.Admin)) && selectedRole != nameof(BTRoles.Admin))
            {
                return Json(new { success = false });
            }

            if (await _roleService.RemoveUserFromRolesAsync(user, currentRoles))
            {
                try
                {
                    if (selectedRole == nameof(BTRoles.ProjectManager))
                    {
                        List<Project> projectsToRemoveUserfrom = await _projectService.GetAllUserProjectsAsync(user.Id);
                        foreach (Project project in projectsToRemoveUserfrom)
                        {
                            await _projectService.RemoveMemberFromProjectAsync(user, project.Id, companyId);
                        }
                    }

                    await _roleService.AddUserToRoleAsync(user, selectedRole);
                }
                catch (Exception)
                {
                    return Json(new { success = false });
                }
            }
            else
            {
                return Json(new { success = false });
            }

            if (currentRoles.FirstOrDefault() == nameof(BTRoles.ProjectManager))
            {

                List<Project> projects = await _projectService.GetAllUserProjectsAsync(user.Id);
                foreach (Project project in projects)
                {
                    try
                    {
                        await _projectService.RemoveProjectManagerAsync(project.Id, companyId);
                    }
                    catch (Exception)
                    {
                        return Json(new { success = false });
                    }
                }
            }

            if (currentRoles.FirstOrDefault() == nameof(BTRoles.Developer))
            {

                List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(user.Id);
                foreach (Ticket ticket in tickets)
                {
                    try
                    {
                        Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                        ticket.DeveloperUserId = null;
                        await _ticketService.UpdateTicketAsync(ticket, companyId);
                        await _ticketHistoryService.AddHistoryAsync(oldTicket, ticket, currentUserId!);
                    }
                    catch (Exception)
                    {
                        return Json(new { success = false });
                    }
                }
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> CheckForAssignments(string? id, string? roleName, string? userName)
        {
            int companyId = User.Identity!.GetCompanyId();

            if (string.IsNullOrEmpty(id))
            {
                return Json(new { changeRole = false });
            }

            BTUser? user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return Json(new { changeRole = false });
            }

            if (!string.IsNullOrEmpty(roleName))
            {
                if (await _roleService.IsUserInRole(user, roleName))
                {
                    return Json(new { changeRole = false, userId = user.Id, selectedRole = roleName });
                }
            }

            if (await _roleService.IsUserInRole(user, nameof(BTRoles.ProjectManager)))
            {
                List<Project> userProjects = await _projectService.GetAllUserProjectsAsync(user.Id);

                return Json(new { userProjects, userId = user.Id, changeRole = true, currentRole = nameof(BTRoles.ProjectManager), userName, selectedRole = roleName });
            }

            else if (await _roleService.IsUserInRole(user, nameof(BTRoles.Developer)))
            {
                List<Ticket> userTickets = (await _ticketService.GetTicketsByCompanyIdAsync(companyId)).Where(t => t.DeveloperUserId == user.Id).ToList();

                return Json(new { userTickets, userId = user.Id, changeRole = true, currentRole = nameof(BTRoles.Developer), userName, selectedRole = roleName });
            }

            else if (await _roleService.IsUserInRole(user, nameof(BTRoles.Submitter)))
            {
                return Json(new { changeRole = true, userId = user.Id, currentRole = nameof(BTRoles.Submitter), selectedRole = roleName });
            }

            else
            {
                List<BTUser> admin = await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Admin), companyId);
                if (admin.Count > 1)
                {
                    return Json(new { changeRole = true, userId = user.Id, currentRole = nameof(BTRoles.Admin), selectedRole = roleName });
                }
                else
                {
                    return Json(new { changeRole = false, userId = user.Id, currentRole = nameof(BTRoles.Admin), selectedRole = roleName });
                }
            }
        }
    }
}