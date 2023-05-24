using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public IFormFile ImageFormFile { get; set; }
        public byte[] ImageFileData { get; set; }
        public string ImageFileType { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsArchived { get; set; }

        // Foreign Keys
        public int CompanyId { get; set; }
        public int ProjectPriorityId { get; set; }

        // Navigation Properties
        public virtual Company Company { get; set; }
        public virtual ProjectPriority ProjectPriority { get; set; }
        public virtual ICollection<BTUser> Members { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }  
    }
}
