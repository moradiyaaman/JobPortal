using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.Models
{
    public class Job
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(160)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [MaxLength(200)]
        public string Qualifications { get; set; }

        [MaxLength(200)]
        public string Experience { get; set; }

        [MaxLength(200)]
        public string Skills { get; set; }

    [MaxLength(120)]
    public string CompanyName { get; set; }

    [MaxLength(200)]
    public string CompanyWebsite { get; set; }

    [MaxLength(200)]
    public string CompanyLogoPath { get; set; }

    public string ProviderId { get; set; }
    public ApplicationUser Provider { get; set; }

    [MaxLength(200)]
    public string ProviderDisplayName { get; set; }

    [MaxLength(300)]
    public string ProviderSummary { get; set; }

        [MaxLength(120)]
        public string Location { get; set; }

        [MaxLength(80)]
        public string Country { get; set; }

        [MaxLength(50)]
        public string JobType { get; set; }

        [MaxLength(80)]
        public string Salary { get; set; }

        public DateTime PostedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public ICollection<JobApplication> Applications { get; set; }
    }
}
