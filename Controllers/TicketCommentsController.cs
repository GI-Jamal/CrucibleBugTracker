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

        //// GET: TicketComments
        //public async Task<IActionResult> Index()
        //{
        //    var applicationDbContext = _context.TicketComments.Include(t => t.Ticket).Include(t => t.User);
        //    return View(await applicationDbContext.ToListAsync());
        //}

        //// GET: TicketComments/Details/5
        //public async Task<IActionResult> Details(int? id)
        //{
        //    if (id == null || _context.TicketComments == null)
        //    {
        //        return NotFound();
        //    }

        //    var ticketComment = await _context.TicketComments
        //        .Include(t => t.Ticket)
        //        .Include(t => t.User)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (ticketComment == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(ticketComment);
        //}

        // GET: TicketComments/Create
        //public IActionResult Create()
        //{
        //    ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description");
        //    ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
        //    return View();
        //}

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

        //// GET: TicketComments/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null || _context.TicketComments == null)
        //    {
        //        return NotFound();
        //    }

        //    var ticketComment = await _context.TicketComments.FindAsync(id);
        //    if (ticketComment == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);
        //    ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.UserId);
        //    return View(ticketComment);
        //}

        //// POST: TicketComments/Edit/5
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        //{
        //    if (id != ticketComment.Id)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(ticketComment);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!TicketCommentExists(ticketComment.Id))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);
        //    ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.UserId);
        //    return View(ticketComment);
        //}

        //// GET: TicketComments/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null || _context.TicketComments == null)
        //    {
        //        return NotFound();
        //    }

        //    var ticketComment = await _context.TicketComments
        //        .Include(t => t.Ticket)
        //        .Include(t => t.User)
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (ticketComment == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(ticketComment);
        //}

        //// POST: TicketComments/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    if (_context.TicketComments == null)
        //    {
        //        return Problem("Entity set 'ApplicationDbContext.TicketComments'  is null.");
        //    }
        //    var ticketComment = await _context.TicketComments.FindAsync(id);
        //    if (ticketComment != null)
        //    {
        //        _context.TicketComments.Remove(ticketComment);
        //    }
            
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        //private bool TicketCommentExists(int id)
        //{
        //  return (_context.TicketComments?.Any(e => e.Id == id)).GetValueOrDefault();
        //}
    }
}
