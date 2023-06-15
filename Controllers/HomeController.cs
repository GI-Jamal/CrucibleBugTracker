using CrucibleBugTracker.Enums;
using CrucibleBugTracker.Extensions;
using CrucibleBugTracker.Models;
using CrucibleBugTracker.Models.ViewModels;
using CrucibleBugTracker.Services;
using CrucibleBugTracker.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Text.Json;

namespace CrucibleBugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRoleService _roleService;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketHistoryService _ticketHistoryService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTCompanyService _companyService;

        public HomeController(ILogger<HomeController> logger,
                              UserManager<BTUser> userManager,
                              IBTTicketService ticketService,
                              IBTProjectService projectService,
                              IBTRoleService roleService,
                              IBTFileService fileService,
                              IBTTicketHistoryService historyService,
                              IBTNotificationService notificationService,
                              IBTCompanyService companyService)
        {
            _logger = logger;
            _userManager = userManager;
            _ticketService = ticketService;
            _projectService = projectService;
            _roleService = roleService;
            _fileService = fileService;
            _notificationService = notificationService;
            _ticketHistoryService = historyService;
            _companyService = companyService;
        }

        public async Task<IActionResult> Index()
        {
            DashboardViewModel viewModel = new DashboardViewModel();
            List<string> developerIds = new();


            if (User.Identity!.IsAuthenticated)
            {
                string? userId = _userManager.GetUserId(User);
                int companyId = User.Identity!.GetCompanyId();


                viewModel.Company = await _companyService.GetCompanyInfoByUserIdAsync(userId!, companyId);
                viewModel.Projects = await _projectService.GetAllUserProjectsAsNoTrackingAsync(userId!);
                viewModel.Tickets = await _ticketService.GetTicketsByUserIdAsync(userId!);
                viewModel.Members = await _companyService.GetCompanyMembersAsync(companyId);

                developerIds = (await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId)).Select(d => d.Id).ToList();

                ViewData["CompanyDeveloperIds"] = (await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId)).Select(u => u.Id).ToList();
                ViewData["ProjectManagers"] = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), nameof(BTUser.Id), nameof(BTUser.FullName));
            }

            ViewData["ProjectTicketsCount"] = JsonSerializer.Serialize(viewModel.Projects?.OrderBy(p => p.Created).Select(p => p.Tickets.Count(t => !t.Archived)).Take(3).ToList());
            ViewData["UnassignedProjectTicketsCount"] = JsonSerializer.Serialize(viewModel.Projects?.OrderBy(p => p.Created).Select(p => p.Tickets.Count(t => t.DeveloperUserId == null && !t.Archived)).Take(3).ToList());
            ViewData["ProjectNames"] = JsonSerializer.Serialize(viewModel.Projects?.OrderBy(p => p.Created).Select(p => p.Name).Take(3).ToList());
            ViewData["ProjectDevelopers"] = JsonSerializer.Serialize(viewModel.Projects?.OrderBy(p => p.Created).Select(p => p.Members.Count(m => developerIds.Contains(m.Id))).Take(3));

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}