using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        [Required]
        public string Comment { get; set; }
        public DateTime CreatedDate { get; set; }

        // Foreign Keys
        public int TicketId { get; set; }
        public string UserId { get; set; }

        // Navigation Properties
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser User { get; set; }
    }
}
