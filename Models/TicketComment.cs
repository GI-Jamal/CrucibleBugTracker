using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Models
{
    public class TicketComment
    {
        public int Id { get; set; }
        
        [Required]
        public string? Comment { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }


        // Foreign Keys
        public int TicketId { get; set; }
        
        [Required]
        public string? UserId { get; set; }


        // Navigation Properties
        public virtual Ticket? Ticket { get; set; }
        
        public virtual BTUser? User { get; set; }
    }
}
