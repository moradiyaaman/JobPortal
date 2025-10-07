using System;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models
{
    public class SavedJob
    {
        public int Id { get; set; }

        [Required]
        public int JobId { get; set; }
        public Job Job { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.UtcNow;
    }
}
