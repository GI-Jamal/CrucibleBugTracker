using System.ComponentModel.DataAnnotations;
using CrucibleBugTracker.Enums;

namespace CrucibleBugTracker.Models
{
    public class Notification
    {
        public int Id { get; set; }
        
        [Required]
        public string? Title { get; set; }
        
        [Required]
        public string? Message { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }
        
        public bool HasBeenViewed { get; set; }


        // Foreign Keys
        [Required]
        public string? RecipientId { get; set; }
        
        [Required]  
        public string? SenderId { get; set;}
        
        public int ProjectId { get; set; }
        
        public int TicketId { get; set; }


        // Navigation Properties
        public virtual Project? Project { get; set; }
        
        public virtual BTUser? Recipient { get; set; }
        
        public virtual Ticket? Ticket { get; set; }
        
        public virtual BTUser? Sender { get; set; }
        
        public virtual BTNotificationType? NotificationType { get; set; }
    }
}
