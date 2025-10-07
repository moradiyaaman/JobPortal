using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.ViewModels
{
    public class ManageJobAlertsViewModel
    {
        public JobAlertFormModel NewAlert { get; set; } = new JobAlertFormModel();
        public IEnumerable<JobAlertSubscriptionViewModel> Alerts { get; set; } = Array.Empty<JobAlertSubscriptionViewModel>();
        public IEnumerable<string> AvailableCountries { get; set; } = Array.Empty<string>();
        public IEnumerable<string> AvailableJobTypes { get; set; } = Array.Empty<string>();
    }

    public class JobAlertFormModel
    {
        [MaxLength(160)]
        [Display(Name = "Keyword")]
        public string Keyword { get; set; }

        [MaxLength(80)]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [MaxLength(50)]
        [Display(Name = "Job type")]
        public string JobType { get; set; }
    }

    public class JobAlertSubscriptionViewModel
    {
        public int Id { get; set; }
        public string Keyword { get; set; }
        public string Country { get; set; }
        public string JobType { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastNotifiedAt { get; set; }
        public int MatchesSinceLastNotification { get; set; }
    }
}
