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
using Microsoft.AspNetCore.Identity;
using CrucibleBugTracker.Services.Interfaces;
using CrucibleBugTracker.Extensions;
using System.Net.Sockets;
using CrucibleBugTracker.Models.ViewModels;
using System.ComponentModel.Design;

namespace CrucibleBugTracker.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRoleService _roleService;

        public ProjectsController(UserManager<BTUser> userManager, IBTFileService fileService, IBTProjectService projectService, IBTTicketService ticketService, IBTRoleService roleService)
        {
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;
            _ticketService = ticketService;
            _roleService = roleService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            ViewData["Title"] = "All Projects";

            return View(projects);
        }

        [HttpPost]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignPM(int projectId, string projectManagerId)
        {
            int companyId = User.Identity!.GetCompanyId();
            Project? project = await _projectService.GetProjectByIdAsync(projectId, companyId);

            if (project == null) return NotFound();

            BTUser? newProjectManager = await _userManager.FindByIdAsync(projectManagerId);
            if (newProjectManager != null)
            {
                await _projectService.AddProjectManagerAsync(projectManagerId,project.Id, companyId);
                return RedirectToAction(nameof(Details), new { id = projectId });
            }
            else if (string.IsNullOrEmpty(projectManagerId))
            {
                await _projectService.RemoveProjectManagerAsync(projectId, companyId);
                return RedirectToAction(nameof(Details), new { id = projectId });
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            Project? project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            if (project.CompanyId != companyId)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();

            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), nameof(ProjectPriority.Id), nameof(ProjectPriority.Name));
            ViewData["ProjectDevelopers"] = new List<BTUser>(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId));
            ViewData["ProjectSubmitters"] = new List<BTUser>(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId));
            ViewData["ProjectManagers"] = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), nameof(BTUser.Id), nameof(BTUser.FullName));

            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Create([Bind("Name,ImageFormFile,Description,StartDate,EndDate,ProjectPriorityId")] Project project, string? developerIds, string? submitterIds, string? projectManagerId)
        {
            ModelState.Remove(nameof(Project.CompanyId));

            if (ModelState.IsValid)
            {
                project.Created = DateTime.UtcNow;
                project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                BTUser? user = await _userManager.GetUserAsync(User);

                project.CompanyId = user!.CompanyId;

                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                if (User.IsInRole(nameof(BTRoles.ProjectManager)))
                {
                    project.Members.Add(user);
                }

                string[]? developers = developerIds?.Remove(developerIds.Length - 1, 1).Split(',');
                string[]? submitters = submitterIds?.Remove(submitterIds.Length - 1, 1).Split(',');



                if (developers != null)
                {
                    foreach (string developerId in developers)
                    {
                        BTUser? developer = (await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Developer), user.CompanyId)).FirstOrDefault(u => u.Id == developerId);
                        if (developer != null)
                        {
                            project.Members.Add(developer);
                        }
                    }
                }

                if (submitters != null)
                {
                    foreach (string submitterId in submitters)
                    {
                        BTUser? submitter = (await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), user.CompanyId)).FirstOrDefault(u => u.Id == submitterId);
                        if (submitter != null)
                        {
                            project.Members.Add(submitter);
                        }
                    }
                }

                await _projectService.AddProjectAsync(project);

                if (projectManagerId != null)
                {
                    BTUser? projectManager = (await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), user.CompanyId)).FirstOrDefault(u => u.Id == projectManagerId);
                    if (projectManager != null)
                    {
                        await _projectService.AddProjectManagerAsync(projectManager.Id, project.Id, user.CompanyId);
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectDevelopers"] = new MultiSelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Developer), project.CompanyId), nameof(BTUser.Id), nameof(BTUser.FullName), developerIds);
            ViewData["ProjectSubmitters"] = new MultiSelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), project.CompanyId), nameof(BTUser.Id), nameof(BTUser.FullName), submitterIds);
            ViewData["ProjectManagers"] = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), project.CompanyId), nameof(BTUser.Id), nameof(BTUser.FullName), projectManagerId);
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), nameof(ProjectPriority.Id), nameof(ProjectPriority.Name), project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            Project? project = await _projectService.GetProjectByIdAsync((int)id, companyId);

            if (project == null)
            {
                return NotFound();
            }

            if (project.CompanyId != companyId)
            {
                return NotFound();
            }

            string? projectManagerId = (await _projectService.GetProjectManagerAsync(project.Id, companyId))?.Id;
            string? userId = _userManager.GetUserId(User);

            if (!User.IsInRole(nameof(BTRoles.Admin)) && projectManagerId != userId)
            {
                return NotFound();
            }

            List<string> developerIds = (await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(BTRoles.Developer), companyId)).Select(u => u.Id).ToList();
            List<string> submitterIds = (await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(BTRoles.Submitter), companyId)).Select(u => u.Id).ToList();

            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), nameof(ProjectPriority.Id), nameof(ProjectPriority.Name), project.ProjectPriorityId);
            ViewData["ProjectDevelopers"] = new List<BTUser>(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId));
            ViewData["ProjectSubmitters"] = new List<BTUser>(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId));
            ViewData["ProjectManagers"] = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), nameof(BTUser.Id), nameof(BTUser.FullName), projectManagerId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ImageFormFile,ImageFileData,ImageFileType,Description,Created,StartDate,EndDate,Archived,CompanyId,ProjectPriorityId")] Project project, List<string> developerIds, List<string> submitterIds, string? projectManagerId)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            if (project.CompanyId != companyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    string? currentPMId = (await _projectService.GetProjectManagerAsync(project.Id, companyId))?.Id;
                    string? userId = _userManager.GetUserId(User);

                    if (!User.IsInRole(nameof(BTRoles.Admin)) && currentPMId != userId)
                    {
                        return NotFound();
                    }

                    project.Created = DateTime.SpecifyKind(project.Created, DateTimeKind.Utc);
                    project.StartDate = DateTime.SpecifyKind(project.StartDate, DateTimeKind.Utc);
                    project.EndDate = DateTime.SpecifyKind(project.EndDate, DateTimeKind.Utc);

                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }


                    await _projectService.UpdateProjectAsync(project, companyId);

                    project.Tickets = (await _ticketService.GetTicketsByCompanyIdAsync(companyId)).Where(t => t.ProjectId == project.Id).ToList();

                    if (project.Archived == true)
                    {
                        await _projectService.ArchiveProjectAsync(project, companyId);
                    }

                    else
                    {
                        await _projectService.RestoreProjectAsync(project, companyId);
                    }

                    if (User.IsInRole(nameof(BTRoles.Admin)))
                    {
                        if (projectManagerId != null)
                        {
                            BTUser? projectManager = (await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId)).FirstOrDefault(u => u.Id == projectManagerId);
                            if (projectManager != null)
                            {
                                await _projectService.AddProjectManagerAsync(projectManager.Id, project.Id, companyId);
                            }
                        }
                        else
                        {
                            BTUser? projectManager = await _projectService.GetProjectManagerAsync(project.Id, companyId);
                            if (projectManager != null)
                            {
                                await _projectService.RemoveProjectManagerAsync(project.Id, companyId);
                            }
                        }
                    }

                    foreach (BTUser member in await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(BTRoles.Developer), companyId))
                    {
                        project.Members.Remove(member);
                    }

                    foreach (BTUser member in await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(BTRoles.Submitter), companyId))
                    {
                        project.Members.Remove(member);
                    }

                    foreach (string developerId in developerIds)
                    {
                        BTUser? developer = (await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId)).FirstOrDefault(u => u.Id == developerId);
                        if (developer != null)
                        {
                            project.Members.Add(developer);
                        }
                    }

                    foreach (string submitterId in submitterIds)
                    {
                        BTUser? submitter = (await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId)).FirstOrDefault(u => u.Id == submitterId);
                        if (submitter != null)
                        {
                            project.Members.Add(submitter);
                        }
                    }

                    await _projectService.UpdateProjectAsync(project, companyId);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProjectExistsAsync(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), nameof(ProjectPriority.Id), nameof(ProjectPriority.Name), project.ProjectPriorityId);
            ViewData["ProjectDevelopers"] = new MultiSelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId), nameof(BTUser.Id), nameof(BTUser.FullName), developerIds);
            ViewData["ProjectSubmitters"] = new MultiSelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId), nameof(BTUser.Id), nameof(BTUser.FullName), submitterIds);
            ViewData["ProjectManagers"] = new SelectList(await _roleService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId), nameof(BTUser.Id), nameof(BTUser.FullName), projectManagerId);
            return View(project);
        }

        // POST: Projects/Archive/5
        [HttpPost]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int archiveProjectId)
        {
            int companyId = User.Identity!.GetCompanyId();
            string? userId = _userManager.GetUserId(User);
            string? currentPMId = (await _projectService.GetProjectManagerAsync(archiveProjectId, companyId))?.Id;
            Project? project = await _projectService.GetProjectByIdAsync(archiveProjectId, companyId);

            if (project == null || (currentPMId is not null && userId != currentPMId && !User.IsInRole(nameof(BTRoles.Admin))))
            {
                return NotFound();
            }

            await _projectService.ArchiveProjectAsync(project, companyId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize(Roles = $"{nameof(BTRoles.Admin)}, {nameof(BTRoles.ProjectManager)}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int restoreProjectId)
        {
            int companyId = User.Identity!.GetCompanyId();
            string? userId = _userManager.GetUserId(User);
            string? currentPMId = (await _projectService.GetProjectManagerAsync(restoreProjectId, companyId))?.Id;
            Project? project = await _projectService.GetProjectByIdAsync(restoreProjectId, companyId);

            if (project == null || (currentPMId is not null && userId != currentPMId && !User.IsInRole(nameof(BTRoles.Admin))))
            {
                return NotFound();
            }

            await _projectService.RestoreProjectAsync(project, companyId);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MyProjects()
        {
            List<Project> projects = await _projectService.GetAllUserProjectsAsync(_userManager.GetUserId(User)!);
            ViewData["Title"] = "My Projects";
            return View(nameof(Index), projects);
        }

        public async Task<IActionResult> Active()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<Project> projects = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId)).Where(p => p.Archived == false).ToList();
            ViewData["Title"] = "Active Projects";
            return View(nameof(Index), projects);
        }

        public async Task<IActionResult> Archived()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<Project> projects = await _projectService.GetArchivedProjectsByCompanyIdAsync(companyId);
            ViewData["Title"] = "Archived Projects";
            return View(nameof(Index), projects);
        }

        // TODO
        public async Task<IActionResult> Unassigned()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<Project> projects = await _projectService.GetUnassignedProjectsByCompanyIdAsync(companyId);
            ViewData["Title"] = "Unassigned Projects";
            return View(nameof(Index), projects);
        }

        private async Task<bool> ProjectExistsAsync(int id)
        {
            int companyId = User.Identity!.GetCompanyId();
            return await _projectService.GetProjectByIdAsync(id, companyId) != null;
        }
    }
}
