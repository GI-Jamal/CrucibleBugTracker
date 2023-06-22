using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Models
{
    public class Invite
    {
        public int Id { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Name = "Invite Date")]
        [Required]
        public DateTime InviteDate { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Name = "Join Date")]
        public DateTime? JoinDate { get; set; }
        
        public Guid CompanyToken { get; set; }
        
        [Required]
        [Display(Name = "Invitee Email")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 5)]
        public string? InviteeEmail { get; set; }
        
        [Required]
        [Display(Name = "Invitee First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string? InviteeFirstName { get; set; }
        
        [Required]
        [Display(Name = "Invitee Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string? InviteeLastName { get; set; }

        [StringLength(500, ErrorMessage = "{0} must not be greater than {1} characters long.")]
        public string? Message { get; set; }
        
        public bool IsValid { get; set; }


        // Foreign Keys
        public int CompanyId { get; set; }
        
        public int? ProjectId { get; set; }
        
        public string? InviteeId { get; set; }
        
        [Required]
        public string? InvitorId { get; set; }


        // Navigation Properties
        public virtual Company? Company { get; set; }
        
        public virtual BTUser? Invitee { get; set; }
        
        public virtual BTUser? Invitor { get; set; }
        
        public virtual Project? Project { get; set; }
    }
}
