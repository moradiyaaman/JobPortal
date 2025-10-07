using System.ComponentModel.DataAnnotations;

namespace JobPortal.ViewModels
{
    public class ProviderJobFormViewModel
    {
        public int? Id { get; set; }

        [Required]
        [MaxLength(160)]
        [Display(Name = "Job title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Job description")]
        public string Description { get; set; }

        [MaxLength(200)]
        [Display(Name = "Qualifications")]
        public string Qualifications { get; set; }

        [MaxLength(200)]
        [Display(Name = "Experience")]
        public string Experience { get; set; }

        [MaxLength(200)]
        [Display(Name = "Skills")]
        public string Skills { get; set; }

        [MaxLength(80)]
        [Display(Name = "Salary range")]
        public string Salary { get; set; }

        [MaxLength(120)]
        [Display(Name = "Location")]
        public string Location { get; set; }

        [MaxLength(80)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [MaxLength(50)]
        [Display(Name = "Job type")]
        public string JobType { get; set; }

        public bool IsActive { get; set; } = true;

        public string ProviderCompanyName { get; set; }
        public string ProviderCompanyWebsite { get; set; }
        public string ProviderCompanyLocation { get; set; }
        public string ProviderCompanyDescription { get; set; }
    }
}
