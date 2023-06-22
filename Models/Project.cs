using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CrucibleBugTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string? Name { get; set; }
        
        [Required]
        [StringLength(3000, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 1)]
        public string? Description { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }
        
        [NotMapped]
        [Display(Name = "Project Image")]
        public IFormFile? ImageFormFile { get; set; }
        
        public byte[]? ImageFileData { get; set; }
        
        public string? ImageFileType { get; set; }

        public bool Archived { get; set; }


        // Foreign Keys
        public int CompanyId { get; set; }
        
        [Display(Name = "Project Priority")]
        public int ProjectPriorityId { get; set; }


        // Navigation Properties
        [JsonIgnore]
        public virtual Company? Company { get; set; }
        
        [Display(Name = "Project Priority")]
        public virtual ProjectPriority? ProjectPriority { get; set; }

        [JsonIgnore]
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();
        
        [JsonIgnore]
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
