using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrucibleBugTracker.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        public string? FirstName { get; set; }
        [Required]
        [Display(Name = "Last Name")]
        public string? LastName { get; set; }
        [NotMapped]
        [Display(Name = "First Name")]
        public string FullName { get { return $"{FirstName} {LastName}"; } }
        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }

        // Foreign Keys
        public int CompanyId { get; set; }

        // Navigation Properties
        public virtual Company? Company { get; set; }
        public ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    }
}
