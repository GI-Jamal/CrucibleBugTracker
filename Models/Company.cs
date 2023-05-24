using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Models
{
    public class Company
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile ImageFormFile { get; set; }
        public byte[] ImageFileData { get; set; }
        public string ImageFileType { get; set; }

        // Navigation Properties
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<Invite> Invites { get; set; }
        public virtual ICollection<BTUser> Members { get; set; }
    }
}
