using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace JobPortal.ViewModels
{
    public class ProfileViewModel
    {
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        public string Address { get; set; }

        public string Mobile { get; set; }

        public string Country { get; set; }

        public string Headline { get; set; }

        [Display(Name = "Professional summary")]
        public string Summary { get; set; }

        public string Education { get; set; }

        [Display(Name = "Work experience")]
        public string Experience { get; set; }

        public string Skills { get; set; }

        public string ExistingResumeFileName { get; set; }

        [Display(Name = "Upload resume (.pdf/.doc/.docx)")]
        public IFormFile ResumeFile { get; set; }

        public bool IsProvider { get; set; }

        [Display(Name = "Company name")]
        public string CompanyName { get; set; }

        [Display(Name = "Company website")]
        [Url]
        public string CompanyWebsite { get; set; }

        [Display(Name = "Company location")]
        public string CompanyLocation { get; set; }

        [Display(Name = "About the company")]
        [MaxLength(600)]
        public string CompanyDescription { get; set; }

        public string ExistingCompanyLogoPath { get; set; }

        [Display(Name = "Upload company logo (.png/.jpg/.svg)")]
        public IFormFile CompanyLogoFile { get; set; }
    }
}
