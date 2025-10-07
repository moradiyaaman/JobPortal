using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.ViewModels
{
    public class JobFilterViewModel
    {
        [Display(Name = "Keyword")]
        public string Keyword { get; set; }

        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Job type")]
        public string JobType { get; set; }

        [Display(Name = "Posted within")]
        public int? PostedWithinDays { get; set; }

        public IEnumerable<string> AvailableCountries { get; set; }
        public IEnumerable<string> AvailableJobTypes { get; set; }

        public IEnumerable<JobListItemViewModel> Jobs { get; set; }
        public bool CanManageSaved { get; set; }
    }

    public class JobListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }
        public string CompanyLogoPath { get; set; }
        public string CompanyWebsite { get; set; }
        public string Location { get; set; }
        public string Country { get; set; }
        public string JobType { get; set; }
        public string Salary { get; set; }
        public DateTime PostedAt { get; set; }
        public bool IsApplied { get; set; }
        public bool IsSaved { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderSummary { get; set; }
    }
}
