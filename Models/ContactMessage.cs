using System;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(120)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MaxLength(160)]
        public string Subject { get; set; }

        [Required]
        [MaxLength(1500)]
        public string Message { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PreferredContactDate { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
}
