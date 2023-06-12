using CrucibleBugTracker.Enums;
using CrucibleBugTracker.Models;

namespace CrucibleBugTracker.Services.Interfaces
{
    public interface IBTNotificationService
    {
        public Task AddNotificationAsync(Notification? notification);

        public Task NotificationsByRoleAsync(int? companyId, Notification? notification, BTRoles role);

        public Task<List<Notification>> GetNotificationsByUserIdAsync(string? userId);

        public Task<bool> NewDeveloperNotificationAsync(int? ticketId, string? developerId, string? senderId);

        public Task<bool> NewTicketNotificationAsync(int? ticketId, string? senderId);

        public Task<bool> SendEmailNotificationByRoleAsync(int? companyId, Notification? notification, BTRoles role);

        public Task<bool> SendEmailNotificationAsync(Notification? notification, string? emailSubject);

        public Task<bool> TicketUpdateNotificationAsync(int? ticketId, string? developerId, string? senderId = null);
    }
}
