using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool HasBeenViewed { get; set; }

        // Foreign Keys
        [Required]
        public int RecipientId { get; set; }
        [Required]
        public int SenderId { get; set;}
        public int ProjectId { get; set; }
        public int TicketId { get; set; }
        public int NotificationTypeId { get; set; }

        // Navigation Properties
        public virtual Project Project { get; set; }
        public virtual BTUser Recipient { get; set; }
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser Sender { get; set; }
        public virtual NotificationType? NotificationType { get; set; }

    }
}
