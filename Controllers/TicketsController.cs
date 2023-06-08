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
using Microsoft.AspNetCore.Identity;
using CrucibleBugTracker.Services.Interfaces;
using System.Net.Sockets;
using CrucibleBugTracker.Extensions;
using CrucibleBugTracker.Enums;
using CrucibleBugTracker.Services;
using System.ComponentModel.Design;
using Org.BouncyCastle.Bcpg;

namespace CrucibleBugTracker.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTProjectService _projectService;
        private readonly IBTRoleService _roleService;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketHistoryService _ticketHistoryService;

        public TicketsController(UserManager<BTUser> userManager, IBTTicketService ticketService, IBTProjectService projectService, IBTRoleService roleService, IBTFileService fileService, IBTTicketHistoryService historyService)
        {
            _userManager = userManager;
            _ticketService = ticketService;
            _projectService = projectService;
            _roleService = roleService;
            _fileService = fileService;
            _ticketHistoryService = historyService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetTicketsByCompanyIdAsync(companyId);

            ViewData["Title"] = "All Tickets";

            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            Ticket? ticket = await _ticketService.GetTicketByIdAsync((int)id, companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();
            string? userId = _userManager.GetUserId(User);

            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId), nameof(Project.Id), nameof(Project.Name));
            }
            else
            {
                ViewData["ProjectId"] = new SelectList(await _projectService.GetAllUserProjectsAsync(userId!), nameof(Project.Id), nameof(Project.Name));

            }
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPriorities(), nameof(TicketPriority.Id), nameof(TicketPriority.Name));
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypes(), nameof(TicketType.Id), nameof(TicketType.Name));
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,ProjectId,TicketPriorityId,TicketStatusId,TicketTypeId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            ModelState.Remove(nameof(ticket.SubmitterUserId));
            string? userId = _userManager.GetUserId(User);
            int companyId = User.Identity!.GetCompanyId();
            ticket.TicketStatusId = 1;

            if (ModelState.IsValid)
            {
                ticket.Created = DateTime.UtcNow;
                ticket.SubmitterUserId = userId;

                await _ticketService.AddTicketAsync(ticket);

                await _ticketHistoryService.AddHistoryAsync(null, ticket, userId!);
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectId"] = new SelectList(await _projectService.GetAllProjectsByCompanyIdAsync(companyId), nameof(Project.Id), nameof(Project.Name));
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPriorities(), nameof(TicketPriority.Id), nameof(TicketPriority.Name));
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypes(), nameof(TicketType.Id), nameof(TicketType.Name));
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket? ticket = await _ticketService.GetTicketByIdAsync((int)id, companyId);

            if (ticket == null)
            {
                return NotFound();
            }

            if (ticket.Project?.CompanyId != companyId)
            {
                return NotFound();
            }

            string? projectManagerId = (await _projectService.GetProjectManagerAsync(ticket.ProjectId, companyId))?.Id;
            string? userId = _userManager.GetUserId(User);

            if (!User.IsInRole(nameof(BTRoles.Admin)) && projectManagerId != userId)
            {
                return NotFound();
            }


            ViewData["DeveloperUserId"] = new SelectList(await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(BTRoles.Developer), companyId), nameof(BTUser.Id), nameof(BTUser.FullName), ticket.DeveloperUserId);
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPriorities(), nameof(TicketPriority.Id), nameof(TicketPriority.Name), ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatuses(), nameof(TicketStatus.Id), nameof(TicketStatus.Name), ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypes(), nameof(TicketType.Id), nameof(TicketType.Name), ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Archived,ArchivedByProject,ProjectId,TicketPriorityId,TicketStatusId,TicketTypeId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            string? projectManagerId = (await _projectService.GetProjectManagerAsync(ticket.ProjectId, companyId))?.Id;
            string? userId = _userManager.GetUserId(User);

            if (!User.IsInRole(nameof(BTRoles.Admin)) && projectManagerId != userId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);
                    ticket.UpdatedDate = DateTime.UtcNow;

                    if (ticket.Archived == false)
                    {
                        ticket.ArchivedByProject = false;
                    }

                    Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                    await _ticketService.UpdateTicketAsync(ticket, companyId);

                    ticket = (await _ticketService.GetTicketByIdAsync(ticket.Id, companyId))!;

                    await _ticketHistoryService.AddHistoryAsync(oldTicket, ticket, userId!);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExistsAsync(ticket.Id))
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

            ViewData["DeveloperUserId"] = new SelectList(await _projectService.GetProjectMembersByRoleAsync(ticket.ProjectId, nameof(BTRoles.Developer), companyId), nameof(BTUser.Id), nameof(BTUser.FullName), ticket.DeveloperUserId);
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPriorities(), nameof(TicketPriority.Id), nameof(TicketPriority.Name), ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatuses(), nameof(TicketStatus.Id), nameof(TicketStatus.Name), ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypes(), nameof(TicketType.Id), nameof(TicketType.Name), ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Archive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Archive(int archiveTicketId)
        {
            int companyId = User.Identity!.GetCompanyId();            
            string? userId = _userManager.GetUserId(User);

            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(archiveTicketId, companyId);
            Ticket? ticket = await _ticketService.GetTicketByIdAsync(archiveTicketId, companyId);

            if (ticket == null || oldTicket == null)
            {
                return NotFound();
            }

            await _ticketService.ArchiveTicketAsync(ticket, companyId);

            await _ticketHistoryService.AddHistoryAsync(oldTicket, ticket, userId!);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int restoreTicketId)
        {
            int companyId = User.Identity!.GetCompanyId();
            string? userId = _userManager.GetUserId(User);

            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(restoreTicketId, companyId);

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(restoreTicketId, companyId);

            if (ticket == null || oldTicket == null) return NotFound();

            await _ticketService.RestoreTicketAsync(ticket, companyId);

            await _ticketHistoryService.AddHistoryAsync(oldTicket, ticket, userId!);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MyTickets()
        {
            string? userId = _userManager.GetUserId(User);
            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(userId!);
            ViewData["Title"] = "My Tickets";
            return View(nameof(Index), tickets);
        }

        public async Task<IActionResult> Active()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<Ticket> tickets = (await _ticketService.GetTicketsByCompanyIdAsync(companyId)).Where(t => t.Archived == false).ToList();
            ViewData["Title"] = "Active Tickets";
            return View(nameof(Index), tickets);
        }

        public async Task<IActionResult> Archived()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<Ticket> tickets = await _ticketService.GetArchivedTicketsAsync(companyId);
            ViewData["Title"] = "Archived Tickets";
            return View(nameof(Index), tickets);
        }

        public async Task<IActionResult> Unassigned()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<Ticket> tickets = (await _ticketService.GetTicketsByCompanyIdAsync(companyId)).Where(t => t.DeveloperUserId == null).ToList();
            ViewData["Title"] = "Unassigned Tickets";
            return View(nameof(Index), tickets);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = nameof(BTRoles.Admin) + "," + nameof(BTRoles.ProjectManager))]
        public async Task<IActionResult> AssignDev(string developerId, int ticketId)
        {
            int companyId = User.Identity!.GetCompanyId();
            Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticketId, companyId);
            Ticket? ticket = await _ticketService.GetTicketByIdAsync(ticketId, companyId);
            string? userId = _userManager.GetUserId(User);

            ViewData["Title"] = "All Tickets";

            if (oldTicket == null || ticket == null)
            {
                return RedirectToAction(nameof(Index));
            }

            string? projectManagerId = (await _projectService.GetProjectManagerAsync(ticket.ProjectId, companyId))?.Id;

            if (projectManagerId != userId && !User.IsInRole(nameof(BTRoles.Admin)))
            {
                return RedirectToAction(nameof(Index));
            }

            if (string.IsNullOrEmpty(developerId) || await _ticketService.IsUserDeveloperInCompany(developerId, companyId))
            {
                ticket.DeveloperUserId = developerId;

                await _ticketService.UpdateTicketAsync(ticket, companyId);

                ticket = await _ticketService.GetTicketByIdAsync(ticketId, companyId);

                await _ticketHistoryService.AddHistoryAsync(oldTicket, ticket!, userId!);

                return RedirectToAction(nameof(Details), new { id = ticket!.Id });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,BTUserId,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DateTime.UtcNow;
                ticketAttachment.BTUserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";

                await _ticketHistoryService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.BTUserId!);
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment? ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string? fileName = ticketAttachment?.FileName;
            byte[]? fileData = ticketAttachment?.FileData;
            string? ext = Path.GetExtension(fileName)?.Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        private async Task<bool> TicketExistsAsync(int id)
        {
            int companyId = User.Identity!.GetCompanyId();
            return await _ticketService.GetTicketByIdAsync(id, companyId) != null;
        }
    }
}
