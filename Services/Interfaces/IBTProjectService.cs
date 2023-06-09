using CrucibleBugTracker.Models;

namespace CrucibleBugTracker.Services.Interfaces
{
    public interface IBTProjectService
    {
        Task AddProjectAsync(Project project);
        Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);
        Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priority);
        Task<List<Project>> GetAllUserProjectsAsync(string userId);
        Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId);
        Task<Project?> GetProjectByIdAsync(int projectId, int companyId);
        Task<List<ProjectPriority>> GetProjectPrioritiesAsync();
        Task ArchiveProjectAsync(Project project, int companyId);
        Task RestoreProjectAsync(Project project, int companyId);
        Task UpdateProjectAsync(Project project, int companyId);
        Task<BTUser?> GetProjectManagerAsync(int projectId, int companyId);
        Task<string?> GetProjectManagerIdAsync(int projectId, int companyId);
        Task<bool> AddProjectManagerAsync(string userId, int projectId, int companyId);
        Task RemoveProjectManagerAsync(int projectId, int companyId);
        Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName, int companyId);
        Task<bool> AddMemberToProjectAsync(BTUser member, int projectId, int companyId);
        Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId, int companyId);
        Task<List<Project>> GetUnassignedProjectsByCompanyIdAsync(int companyId);
        Task<List<BTUser>> GetProjectMembersByRoleAsNoTrackingAsync(int projectId, string roleName, int companyId);
    }
}
