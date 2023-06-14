using CrucibleBugTracker.Data;
using CrucibleBugTracker.Enums;
using CrucibleBugTracker.Models;
using CrucibleBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace CrucibleBugTracker.Services
{

    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRoleService _roleService;

        public BTTicketService(ApplicationDbContext context, IBTRoleService roleService)
        {
            _context = context;
            _roleService = roleService;
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketCommentAsync(TicketComment comment)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.Include(t => t.Comments).FirstOrDefaultAsync(t => t.Id == comment.TicketId);
                
                ticket?.Comments.Add(comment);

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ArchiveTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                if (ticket.Project!.CompanyId == companyId)
                {
                    ticket.Archived = true;
                    ticket.ArchivedByProject = false;
                    ticket.UpdatedDate = DateTime.UtcNow;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            try
            {
                return await _context.Tickets.Include(t => t.Project)
                                             .Include(t => t.DeveloperUser)
                                             .Include(t => t.SubmitterUser)
                                             .Include(t => t.TicketPriority)
                                             .Include(t => t.TicketStatus)
                                             .Include(t => t.TicketType)
                                             .Where(t => t.Project!.CompanyId == companyId && t.Archived == true)
                                             .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Ticket?> GetTicketAsNoTrackingAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.AsNoTracking().Include(t => t.Project)
                                                   .Include(t => t.DeveloperUser)
                                                   .Include(t => t.SubmitterUser)
                                                   .Include(t => t.TicketPriority)
                                                   .Include(t => t.TicketStatus)
                                                   .Include(t => t.TicketType)
                                                   .FirstOrDefaultAsync(t => t.Project!.CompanyId == companyId && t.Id == ticketId);
                return ticket;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<TicketAttachment?> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.TicketAttachments
                                                                  .Include(t => t.BTUser)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket?> GetTicketByIdAsync(int ticketId, int companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.Include(t => t.Project)
                                                       .Include(t => t.DeveloperUser)
                                                       .Include(t => t.SubmitterUser)
                                                       .Include(t => t.TicketPriority)
                                                       .Include(t => t.TicketStatus)                                                       
                                                       .Include(t => t.Project)
                                                           .ThenInclude(p => p!.Members)
                                                       .Include(t => t.TicketType)
                                                       .Include(t => t.History)
                                                        .ThenInclude(h => h.User)
                                                       .Include(t => t.Attachments)
                                                       .Include(t => t.Comments)
                                                        .ThenInclude(c => c.User)
                                                       .FirstOrDefaultAsync(t => t.Project!.CompanyId == companyId && t.Id == ticketId);
                return ticket;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketPriority>> GetTicketPriorities()
        {
            try
            {
                return await _context.TicketPriorities.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                return await _context.Tickets.Include(t => t.Project)
                                                .ThenInclude(p => p!.Members)
                                             .Include(t => t.DeveloperUser)
                                             .Include(t => t.SubmitterUser)
                                             .Include(t => t.TicketType)
                                             .Include(t => t.TicketStatus)
                                             .Include(t => t.TicketPriority)
                                             .Where(t => t.Project!.CompanyId == companyId)
                                             .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId)
        {
            try
            {
                BTUser? user = await _context.Users.FindAsync(userId);
                if (user is null)
                {
                    return new List<Ticket>();
                }

                if (await _roleService.IsUserInRole(user, nameof(BTRoles.Admin)))
                {
                    return await GetTicketsByCompanyIdAsync(user.CompanyId);
                }

                else if (await _roleService.IsUserInRole(user, nameof(BTRoles.ProjectManager)))
                {
                    return await _context.Tickets.Include(t => t.Project)
                                                            .ThenInclude(p => p!.Members)
                                                           .Include(t => t.DeveloperUser)
                                                           .Include(t => t.SubmitterUser)
                                                           .Include(t => t.TicketPriority)
                                                           .Include(t => t.TicketStatus)
                                                           .Include(t => t.TicketType)
                                                           .Where(t => !t.Archived && t.Project!.Members.Any(m => m.Id == userId))
                                                           .ToListAsync();
                }
                else
                {
                    return await _context.Tickets.Include(t => t.Project)
                                                            .ThenInclude(p => p!.Members)
                                                           .Include(t => t.DeveloperUser)
                                                           .Include(t => t.SubmitterUser)
                                                           .Include(t => t.TicketPriority)
                                                           .Include(t => t.TicketStatus)
                                                           .Include(t => t.TicketType)
                                                           .Where(t => !t.Archived && (t.DeveloperUserId == userId || t.SubmitterUserId == userId))
                                                           .ToListAsync();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketStatus>> GetTicketStatuses()
        {
            try
            {
                return await _context.TicketStatuses.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Ticket>> GetTicketsToBeAltered(List<string> newDevIds, int projectId, int companyId)
        {
            try
            {
                List<Ticket> oldAllCurrentlyAssignedTicketsInProject = await _context.Tickets.AsNoTracking()
                                                                                             .Include(t => t.DeveloperUser)
                                                                                             .AsNoTracking()
                                                                                             .Where(t => t.ProjectId == projectId && t.Project!.CompanyId == companyId && t.DeveloperUserId != null)
                                                                                             .OrderBy(t => t.Archived).ThenBy(t => t.DeveloperUser!.FirstName)
                                                                                             .ToListAsync();

                List<Ticket> newAllCurrentlyAssignedTicketsInProject = new();

                foreach (string devId in newDevIds)
                {
                    List<Ticket> tickets = await _context.Tickets.Where(t => t.DeveloperUserId == devId && t.ProjectId == projectId && t.Project!.CompanyId == companyId).AsNoTracking().ToListAsync();
                    newAllCurrentlyAssignedTicketsInProject.AddRange(tickets);
                }

                List<int> ticketIds = newAllCurrentlyAssignedTicketsInProject.Select(t => t.Id).ToList();

                List<Ticket> ticketsToBeAltered = new();

                foreach (Ticket ticket in oldAllCurrentlyAssignedTicketsInProject)
                {
                    if (!ticketIds.Contains(ticket.Id))
                    {
                        ticketsToBeAltered.Add(ticket);
                    }
                }

                return ticketsToBeAltered;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketType>> GetTicketTypes()
        {
            try
            {
                return await _context.TicketTypes.ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<Ticket>> GetUnassignedTicketsAsync(int companyId)
        {
            try
            {
                return _context.Tickets.Where(t => t.DeveloperUserId == null).ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> IsUserDeveloperInCompany(string userId, int companyId)
        {
            BTUser? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);

            if (user is not null)
            {
                if (user.CompanyId == companyId && await _roleService.IsUserInRole(user, nameof(BTRoles.Developer)))
                {
                    return true;
                }

                else
                {
                    return false;
                }
            }

            return false;
        }            

        public async Task RestoreTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                if (ticket.Project?.CompanyId == companyId)
                {
                    ticket.Archived = false;
                    ticket.ArchivedByProject = false;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket, int companyId)
        {
            try
            {
                if (await _context.Projects.AnyAsync(p => p.Id == ticket.ProjectId && p.CompanyId == companyId))
                {
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
