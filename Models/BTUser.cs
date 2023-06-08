using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CrucibleBugTracker.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; }
        
        [Required]
        [Display(Name = "Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }
        
        [NotMapped]
        [Display(Name = "Full Name")]
        public string FullName { get { return $"{FirstName} {LastName}"; } }
       
        [NotMapped]
        [Display(Name = "User Image")]
        public IFormFile? ImageFormFile { get; set; }
        
        public byte[]? ImageFileData { get; set; }
        
        public string? ImageFileType { get; set; }


        // Foreign Keys
        public int CompanyId { get; set; }

        //public int? DepartmentId { get; set; }


        // Navigation Properties
        //public virtual Department? Department { get; set; }
        [JsonIgnore]
        public virtual Company? Company { get; set; }
        [JsonIgnore]
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
    }
}
