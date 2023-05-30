using CrucibleBugTracker.Data;
using CrucibleBugTracker.Models;
using CrucibleBugTracker.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrucibleBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;

        public BTProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddProjectAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
        }

        public async Task ArchiveProjectAsync(Project project, int companyId)
        {
            if (project.CompanyId == companyId)
            {
                project.Archived = true;
                foreach (Ticket ticket in project.Tickets)
                {
                    if (ticket.Archived == false && ticket.ArchivedByProject == false)
                    {
                        ticket.Archived = true;
                        ticket.ArchivedByProject = true;
                        _context.Update(ticket);
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            return await _context.Projects.Where(p => p.CompanyId == companyId).Include(p => p.Company).Include(p => p.ProjectPriority).ToListAsync();
        }

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priority)
        {
            return await _context.Projects.Include(p => p.ProjectPriority).Where(p => p.CompanyId == companyId && p.ProjectPriority!.Name == priority).ToListAsync();
        }

        public async Task<List<Project>> GetAllUserProjectsAsync(string userId)
        {
            return await _context.Projects.Include(p => p.Members).Where(p => p.Members.Any(m => m.Id == userId)).ToListAsync();
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            return await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true).ToListAsync();
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId, int companyId)
        {
            Project? project = await _context.Projects.Include(p => p.Company)
                                                      .Include(p => p.ProjectPriority)
                                                      .Include(p => p.Tickets).AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
            if (project != null)
            {
                return project;
            }
            else
            {
                throw new Exception("Project not found.");
            }
        }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            return await _context.ProjectPriorities.ToListAsync();
        }

        public async Task RestoreProjectAsync(Project project, int companyId)
        {
            if (project.CompanyId == companyId)
            {
                project.Archived = false;
                foreach (Ticket ticket in project.Tickets)
                {
                    if (ticket.Archived == true && ticket.ArchivedByProject == true)
                    {
                        ticket.Archived = false;
                        ticket.ArchivedByProject = false;
                        _context.Update(ticket);
                    }
                }
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateProjectAsync(Project project, int companyId)
        {
            if (project.CompanyId == companyId)
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
        }
    }
}
