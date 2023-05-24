using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }
        public DateTime InviteDate { get; set; }
        public DateTime? JoinDate { get; set; }
        public Guid CompanyToken { get; set; }
        [Required]
        public string InviteeEmail { get; set; }
        [Required]
        public string InviteeFirstName { get; set; }
        [Required]
        public string InviteeLastName { get; set; }
        public string Message { get; set; }
        public bool IsValid { get; set; }

        // Foreign Keys
        public int CompanyId { get; set; }
        public string? InviteeId { get; set; }
        [Required]
        public string? InvitorId { get; set; }
        public string? ProjectId { get; set; }

        // Navigation Properties
        public virtual Company Company { get; set; }
        public virtual BTUser Invitee { get; set; }
        public virtual BTUser Invitor { get; set; }
        public virtual Project Project { get; set; }
    }
}
