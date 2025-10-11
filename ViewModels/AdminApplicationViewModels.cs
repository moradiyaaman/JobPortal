using System;
using System.Collections.Generic;

namespace JobPortal.ViewModels
{
    public class AdminApplicationsIndexViewModel
    {
        public string SortOrder { get; set; }
        public bool HasAtsData { get; set; }
        public IEnumerable<AdminApplicationListItemViewModel> Applications { get; set; }
    }

    public class AdminApplicationListItemViewModel
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string ApplicantName { get; set; }
        public string ApplicantEmail { get; set; }
        public DateTime AppliedAt { get; set; }
        public string Status { get; set; }
        public string ResumeUrl { get; set; }
        public string CoverLetter { get; set; }
        public int? AtsScore { get; set; }
        public IReadOnlyCollection<string> AtsMatchedKeywords { get; set; }
        public IReadOnlyCollection<string> AtsMissingKeywords { get; set; }
    }
}
