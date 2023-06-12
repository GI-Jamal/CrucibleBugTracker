using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CrucibleBugTracker.Data;
using CrucibleBugTracker.Models;
using CrucibleBugTracker.Services;
using Microsoft.AspNetCore.Identity;
using CrucibleBugTracker.Services.Interfaces;

namespace CrucibleBugTracker.Controllers
{
    public class TicketCommentsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTTicketHistoryService _ticketHistoryService;

        public TicketCommentsController(UserManager<BTUser> userManager, IBTProjectService projectService, IBTTicketService ticketService, IBTTicketHistoryService historyService)
        {
            _ticketHistoryService = historyService;
            _ticketService = ticketService;
            _projectService = projectService;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        {
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                BTUser? commenter = await _userManager.GetUserAsync(User);

                Ticket? ticket = await _ticketService.GetTicketAsNoTrackingAsync(ticketComment.TicketId, commenter!.CompanyId);

                if (ticket == null)
                {
                    return NotFound();
                }
                if (!User.IsInRole("Admin") &&
                    (await _projectService.GetProjectManagerAsync(ticket.ProjectId, commenter.CompanyId))?.Id != commenter.Id &&
                    ticket.DeveloperUserId != commenter.Id &&
                    ticket.SubmitterUserId != commenter.Id)
                {
                    return RedirectToAction("Details", "Tickets", new { Id = ticketComment.TicketId });
                }

                ticketComment.UserId = commenter.Id;
                ticketComment.Created = DateTime.UtcNow;
                await _ticketService.AddTicketCommentAsync(ticketComment);

                await _ticketHistoryService.AddHistoryAsync(ticketComment.TicketId, nameof(TicketComment), ticketComment.UserId);
            }

            return RedirectToAction("Details", "Tickets", new { Id = ticketComment.TicketId });
        }
    }
}
