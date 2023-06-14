using CrucibleBugTracker.Models;

namespace CrucibleBugTracker.Services.Interfaces
{
    public interface IBTCompanyService
    {
        Task<Company?> GetCompanyInfoAsync(int? companyId);
        Task<Company?> GetCompanyInfoByUserIdAsync(string userId, int companyId);
        Task<List<BTUser>> GetCompanyMembersAsync(int companyId);
        Task UpdateCompanyAsync(Company company, int companyId);
    }
}
