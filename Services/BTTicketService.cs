using CrucibleBugTracker.Data;
using CrucibleBugTracker.Models;
using CrucibleBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrucibleBugTracker.Services
{

    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
        }

        public async Task ArchiveTicketAsync(Ticket ticket, int companyId)
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

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int companyId)
        {
            return await _context.Tickets.Include(t => t.Project).Where(t => t.Project!.CompanyId == companyId && t.Archived == true).ToListAsync();
        }

        public async Task<Ticket?> GetTicketByIdAsync(int ticketId, int companyId)
        {
            Ticket? ticket = await _context.Tickets.Include(t => t.Project)
                                                   .Include(t => t.DeveloperUser)
                                                   .Include(t => t.SubmitterUser)
                                                   .Include(t => t.TicketPriority)
                                                   .Include(t => t.TicketStatus)
                                                   .Include(t => t.TicketType)
                                                   .FirstOrDefaultAsync(t => t.Project!.CompanyId == companyId && t.Id == ticketId);
            return ticket!;
        }

        public async Task<List<TicketPriority>> GetTicketPriorities()
        {
            return await _context.TicketPriorities.ToListAsync();
        }

        public async Task<List<Ticket>> GetTicketsByCompanyIdAsync(int companyId)
        {
            return await _context.Tickets.Include(t => t.Project).Include(t => t.DeveloperUser).Include(t => t.SubmitterUser).Where(t => t.Project!.CompanyId == companyId).ToListAsync();
        }

        public async Task<List<TicketStatus>> GetTicketStatuses()
        {
            return await _context.TicketStatuses.ToListAsync();
        }

        public async Task<List<TicketType>> GetTicketTypes()
        {
            return await _context.TicketTypes.ToListAsync();
        }

        public async Task RestoreTicketAsync(Ticket ticket, int companyId)
        {
            if (ticket.Project?.CompanyId == companyId)
            {
                ticket.Archived = false;
                ticket.ArchivedByProject = false;
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket, int companyId)
        {
            if (await _context.Projects.AnyAsync(p => p.Id == ticket.ProjectId && p.CompanyId == companyId))
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
        }
    }
}
