using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CrucibleBugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        
        public string? Description { get; set; }
        
        [DataType(DataType.DateTime)]
        [Display(Name = "Created Date")]
        public DateTime Created { get; set; }
        
        [NotMapped]
        [Display(Name = "Attached Ticket File")]
        public IFormFile? FormFile { get; set; }
        
        public byte[]? FileData { get; set; }
        
        public string? FileType { get; set; }


        // Foreign Keys
        public int TicketId { get; set; }
        
        [Required]
        public string? BTUserId { get; set; }


        // Navigation Properties
        public virtual Ticket? Ticket { get; set; }
        
        public virtual BTUser? BTUser { get; set; }    
    }
}
