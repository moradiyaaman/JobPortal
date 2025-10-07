using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JobPortal.Models
{
    public class JobApplication
    {
        public int Id { get; set; }

        [Required]
        public int JobId { get; set; }
        public Job Job { get; set; }

        [Required]
        public string ApplicantId { get; set; }
        public ApplicationUser Applicant { get; set; }

        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(60)]
        public string Status { get; set; } = "Applied";
    }
}
