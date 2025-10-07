using System;

namespace JobPortal.ViewModels
{
    public class JobDetailViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Qualifications { get; set; }
        public string Experience { get; set; }
        public string Skills { get; set; }
        public string Salary { get; set; }
        public string CompanyName { get; set; }
        public string CompanyWebsite { get; set; }
        public string CompanyLogoPath { get; set; }
        public string Location { get; set; }
        public string Country { get; set; }
        public string JobType { get; set; }
        public DateTime PostedAt { get; set; }
        public bool IsApplied { get; set; }
        public bool CanApply { get; set; }
        public bool CanSave { get; set; }
        public bool IsSaved { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderSummary { get; set; }
        public string ProviderWebsite { get; set; }
    }
}
