using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrucibleBugTracker.Models
{
    public class Company
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string? Name { get; set; }
        
        [StringLength(500, ErrorMessage = "The {0} must not be greater than {1} characters long.")]
        public string? Description { get; set; }
        
        [NotMapped]
        [Display(Name = "Company Image")]
        public IFormFile? ImageFormFile { get; set; }
        
        public byte[]? ImageFileData { get; set; }
        
        public string? ImageFileType { get; set; }


        // Navigation Properties
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
        
        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();
        
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();
    }
}
