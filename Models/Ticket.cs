using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? Title { get; set; }
        
        [Required]
        public string? Description { get; set;}
        
        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Name = "Updated Date")]
        public DateTime? UpdatedDate { get; set; }
        
        public bool Archived { get; set; }

        [Display(Name = "Archived By Project")]
        public bool ArchivedByProject { get; set; }


        // Foreign Keys
        [Display(Name = "Project")]
        public int ProjectId { get; set; }
        
        [Display(Name = "Ticket Priority")]
        public int TicketPriorityId { get; set; }
        
        [Display(Name = "Ticket Status")]
        public int TicketStatusId { get; set; }
        
        [Display(Name = "Ticket Type")]
        public int TicketTypeId { get; set; }
        
        [Display(Name = "Developer User")]
        public string? DeveloperUserId { get; set; }
        
        [Required]
        public string? SubmitterUserId { get; set; }


        // Navigation Properties
        public virtual Project? Project { get; set; }
        
        public virtual TicketPriority? TicketPriority { get; set; }
        
        public virtual TicketStatus? TicketStatus { get; set; }
        
        public virtual TicketType? TicketType { get; set; }
        
        public virtual BTUser? DeveloperUser { get; set; }
        
        public virtual BTUser? SubmitterUser { get; set; }
        
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();
        
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();
        
        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();
    }
}
