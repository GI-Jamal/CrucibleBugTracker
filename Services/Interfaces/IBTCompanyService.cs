using CrucibleBugTracker.Models;

namespace CrucibleBugTracker.Services.Interfaces
{
    public interface IBTCompanyService
    {
        Task<Company?> GetCompanyInfoAsync(int? companyId);
        Task<List<BTUser>> GetCompanyMembersAsync(int companyId);
    }
}
