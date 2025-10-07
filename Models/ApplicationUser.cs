using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace JobPortal.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Address { get; set; }
        public string Mobile { get; set; }
        public string Country { get; set; }
        public string Headline { get; set; }
        public string Summary { get; set; }
        public string Education { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }
        public string ResumeFileName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsAdmin { get; set; }
        public bool IsProvider { get; set; }
        public string CompanyName { get; set; }
        public string CompanyWebsite { get; set; }
        public string CompanyDescription { get; set; }
        public string CompanyLocation { get; set; }
        public string CompanyLogoPath { get; set; }

        public ICollection<JobApplication> Applications { get; set; }
        public ICollection<Job> JobsPosted { get; set; }
    }
}
