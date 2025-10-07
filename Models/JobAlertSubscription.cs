using System;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models
{
    public class JobAlertSubscription
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [MaxLength(160)]
        public string Keyword { get; set; }

        [MaxLength(80)]
        public string Country { get; set; }

        [MaxLength(50)]
        public string JobType { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastNotifiedAt { get; set; }
    }
}
