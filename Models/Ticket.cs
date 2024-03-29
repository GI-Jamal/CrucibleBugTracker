﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CrucibleBugTracker.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(60, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string? Title { get; set; }
        
        [Required]
        [StringLength(1000, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
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
        
        [Display(Name = "Ticket Priority")]
        
        public virtual TicketPriority? TicketPriority { get; set; }
        
        [Display(Name = "Ticket Status")]
        
        public virtual TicketStatus? TicketStatus { get; set; }
        
        [Display(Name = "Ticket Type")]
        
        public virtual TicketType? TicketType { get; set; }
        
        [Display(Name = "Developer")]
        
        public virtual BTUser? DeveloperUser { get; set; }
        
        [Display(Name = "Submitter")]
        
        public virtual BTUser? SubmitterUser { get; set; }
        
        
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();
        
        
        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();
        
        
        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();
    }
}
