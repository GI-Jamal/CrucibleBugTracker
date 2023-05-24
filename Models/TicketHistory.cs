using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Models
{
    public class TicketHistory
    {
        public int Id { get; set; }
        
        [Display(Name = "Property Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string? PropertyName { get; set; }
        
        [StringLength(50, ErrorMessage = "The {0} must be at max {1} characters long.")]
        public string? Description { get; set; }
        
        public string? OldValue { get; set; }
        
        public string? NewValue { get; set; }
        
        [DataType(DataType.DateTime)]
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
