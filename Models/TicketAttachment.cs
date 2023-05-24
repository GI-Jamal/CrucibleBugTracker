using System.ComponentModel.DataAnnotations;

namespace CrucibleBugTracker.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public IFormFile FormFile { get; set; }
        public byte[] FileData { get; set; }
        public string FileType { get; set; }

        // Foreign Keys
        public int TicketId { get; set; }
        [Required]
        public string BTUserId { get; set; }

        // Navigation Properties
        public virtual Ticket Ticket { get; set; }
        public virtual BTUser BTUser { get; set; }    
    }
}
