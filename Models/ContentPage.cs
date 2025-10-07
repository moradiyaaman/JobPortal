using System;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models
{
    public class ContentPage
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Slug { get; set; }

        [Required]
        [MaxLength(160)]
        public string Title { get; set; }

        [Required]
        public string Body { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
