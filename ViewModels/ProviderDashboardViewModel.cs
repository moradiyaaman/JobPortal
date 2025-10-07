using System;
using System.Collections.Generic;

namespace JobPortal.ViewModels
{
    public class ProviderDashboardViewModel
    {
        public string CompanyName { get; set; }
        public string CompanyLocation { get; set; }
        public string CompanyWebsite { get; set; }
        public string CompanyDescription { get; set; }
        public int ActiveJobs { get; set; }
        public int TotalJobs { get; set; }
        public int TotalApplications { get; set; }
        public IEnumerable<ProviderJobListItemViewModel> RecentJobs { get; set; }
        public IEnumerable<ProviderRecentApplicationViewModel> RecentApplications { get; set; }
    }

    public class ProviderJobListItemViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsActive { get; set; }
        public DateTime PostedAt { get; set; }
        public int ApplicationCount { get; set; }
    }

    public class ProviderRecentApplicationViewModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string ApplicantName { get; set; }
        public DateTime AppliedAt { get; set; }
        public string Status { get; set; }
    }
}
