using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public string PropertyName { get; set; }
        public string Description { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime CreatedDate { get; set; }

        // Foreign Keys
        public int TicketId { get; set; }
        [Required]
        public string UserId { get; set; }

        // Navigation Properties
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser User { get; set; }
    }
}
