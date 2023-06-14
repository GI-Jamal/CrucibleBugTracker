using CrucibleBugTracker.Data;
using CrucibleBugTracker.Enums;
using CrucibleBugTracker.Models;
using CrucibleBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace CrucibleBugTracker.Services
{
    public class BTCompanyService : IBTCompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRoleService _roleService;

        public BTCompanyService(ApplicationDbContext context, IBTRoleService roleService)
        {
            _context = context;
            _roleService = roleService;
        }

        public async Task<Company?> GetCompanyInfoAsync(int? companyId)
        {
            try
            {
                Company? company = await _context.Companies.Include(c => c.Members)
                                                           .Include(c => c.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                           .Include(c => c.Invites)
                                                           .Include(c => c.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.TicketStatus)
                                                            .Include(c => c.Projects)
                                                                .ThenInclude(p => p.Members)
                                                            .Include(c => c.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.TicketType)
                                                           .FirstOrDefaultAsync(c => c.Id == companyId);
                return company;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Company?> GetCompanyInfoByUserIdAsync(string userId, int companyId)
        {
            try
            {
                BTUser? user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return null;
                }


                if (await _roleService.IsUserInRole(user, nameof(BTRoles.Admin)))
                {
                    Company? company = await _context.Companies.Include(c => c.Members)
                                                           .Include(c => c.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                           .Include(c => c.Invites)
                                                           .Include(c => c.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.TicketStatus)
                                                            .Include(c => c.Projects)
                                                                .ThenInclude(p => p.Members)
                                                            .Include(c => c.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.TicketType)
                                                           .FirstOrDefaultAsync(c => c.Id == companyId);
                    return company;
                }

                else
                {
                    Company? company = await _context.Companies.Include(c => c.Members)
                                                           .Include(c => c.Projects.Where(p => p.Members.Contains(user)))
                                                                .ThenInclude(p => p.Tickets)
                                                           .Include(c => c.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.TicketStatus)
                                                            .Include(c => c.Projects)
                                                                .ThenInclude(p => p.Members)
                                                            .Include(c => c.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                    .ThenInclude(t => t.TicketType)
                                                           .FirstOrDefaultAsync(c => c.Id == companyId);
                    return company;
                }                
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BTUser>> GetCompanyMembersAsync(int companyId)
        {
            try
            {
                List<BTUser> users = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();

                return users;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateCompanyAsync(Company company, int companyId)
        {
            try
            {
                if (company.Id == companyId)
                {
                    _context.Update(company);
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
