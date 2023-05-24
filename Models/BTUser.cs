using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
        public IFormFile ImageFile { get; set; }
        public byte[] ImageFileData { get; set; }
        public string ImageFileType { get; set; }

        // Foreign Keys
        public int? CompanyId { get; set; }

        // Navigation Properties
        public virtual Company Company { get; set; }
        public ICollection<Project> Projects { get; set; }
    }
}
