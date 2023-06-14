using CrucibleBugTracker.Data;
using CrucibleBugTracker.Models;
using CrucibleBugTracker.Services.Interfaces;
using CrucibleBugTracker.Enums;
using Microsoft.EntityFrameworkCore;

namespace CrucibleBugTracker.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRoleService _roleService;

        public BTProjectService(ApplicationDbContext context, IBTRoleService roleService)
        {
            _context = context;
            _roleService = roleService;
        }

        public async Task<bool> AddMemberToProjectAsync(BTUser member, int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is null || member.CompanyId != companyId)
                {
                    return false;

                }

                if (!project.Members.Contains(member))
                {

                    project.Members.Add(member);

                    await _context.SaveChangesAsync();

                    return true;
                }

                return false;


            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
                BTUser? projectManager = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);

                if (project is not null && projectManager is not null)
                {
                    if (await _roleService.IsUserInRole(projectManager, nameof(BTRoles.ProjectManager)))
                    {
                        await RemoveProjectManagerAsync(projectId, companyId);

                        project.Members.Add(projectManager);

                        await _context.SaveChangesAsync();

                        return true;
                    }

                    return false;
                }

                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ArchiveProjectAsync(Project project, int companyId)
        {
            try
            {
                if (project.CompanyId == companyId)
                {
                    project.Archived = true;
                    foreach (Ticket ticket in project.Tickets)
                    {
                        if (ticket.Archived == false)
                        {
                            ticket.Archived = true;
                            ticket.ArchivedByProject = true;
                            _context.Update(ticket);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                return await _context.Projects.Where(p => p.CompanyId == companyId).Include(p => p.Tickets).Include(p => p.Company).Include(p => p.ProjectPriority).ToListAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priority)
        {
            try
            {
                return await _context.Projects.Include(p => p.ProjectPriority).Where(p => p.CompanyId == companyId && p.ProjectPriority!.Name == priority).ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetAllUserProjectsAsync(string userId)
        {
            try
            {
                BTUser? user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

                if (user is not null)
                {
                    if (await _roleService.IsUserInRole(user, nameof(BTRoles.Admin)))
                    {
                        return await GetAllProjectsByCompanyIdAsync(user.CompanyId);
                    }
                }

                return await _context.Projects.Include(p => p.Members).Include(p => p.ProjectPriority).Include(p => p.Company).Where(p => p.Members.Any(m => m.Id == userId)).ToListAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                return await _context.Projects.Where(p => p.CompanyId == companyId && p.Archived == true).Include(p => p.Tickets).Include(p => p.Company).Include(p => p.ProjectPriority).ToListAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Project?> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Company)
                                                      .Include(p => p.ProjectPriority)
                                                      .Include(p => p.Members)
                                                      .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.DeveloperUser)
                                                      .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.SubmitterUser)
                                                      .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.TicketStatus)
                                                      .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.TicketType)
                                                      .Include(p => p.Tickets)
                                                        .ThenInclude(t => t.TicketPriority).FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);
                if (project != null)
                {
                    return project;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<BTUser?> GetProjectManagerAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project != null)
                {
                    foreach (BTUser user in project.Members)
                    {
                        if (await _roleService.IsUserInRole(user, nameof(BTRoles.ProjectManager)))
                        {
                            return user;
                        }
                    }
                }

                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string?> GetProjectManagerIdAsync(int projectId, int companyId)
        {
            try
            {
                string? currentPMId = (await GetProjectManagerAsync(projectId, companyId))?.Id;
                return currentPMId;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName, int companyId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                List<BTUser> projectMembers = new();

                if (project != null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _roleService.IsUserInRole(member, roleName))
                        {
                            projectMembers.Add(member);
                        }
                    }
                }

                return projectMembers;
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public async Task<List<BTUser>> GetProjectMembersByRoleAsNoTrackingAsync(int projectId, string roleName, int companyId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                List<BTUser> projectMembers = new();

                if (project != null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _roleService.IsUserInRole(member, roleName))
                        {
                            projectMembers.Add(member);
                        }
                    }
                }

                return projectMembers;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                return await _context.ProjectPriorities.ToListAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetUnassignedProjectsByCompanyIdAsync(int companyId)
        {
            try
            {

                List<Project> allProjects = await GetAllProjectsByCompanyIdAsync(companyId);
                List<Project> unassignedProjects = new();

                foreach (Project project in allProjects)
                {
                    if (await GetProjectManagerAsync(project.Id, project.CompanyId) == null)
                    {
                        unassignedProjects.Add(project);
                    }
                }
                return unassignedProjects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is null || member.CompanyId != companyId)
                {
                    return false;

                }

                if (project.Members.Contains(member))
                {

                    project.Members.Remove(member);

                    await _context.SaveChangesAsync();

                    return true;
                }

                return false;


            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                if (project is not null)
                {
                    foreach (BTUser member in project.Members)
                    {
                        if (await _roleService.IsUserInRole(member, nameof(BTRoles.ProjectManager)))
                        {
                            project.Members.Remove(member);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RestoreProjectAsync(Project project, int companyId)
        {
            try
            {
                if (project.CompanyId == companyId)
                {
                    project.Archived = false;
                    foreach (Ticket ticket in project.Tickets)
                    {
                        if (ticket.ArchivedByProject == true)
                        {
                            ticket.Archived = false;
                            ticket.ArchivedByProject = false;
                            _context.Update(ticket);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project, int companyId)
        {
            try
            {
                if (project.CompanyId == companyId)
                {
                    _context.Update(project);
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
