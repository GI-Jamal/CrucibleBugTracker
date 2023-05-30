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
    }
}
