using System.Collections.Generic;

namespace JobPortal.ViewModels
{
    public class HomeStatsViewModel
    {
        public int TotalJobs { get; set; }
        public int TotalApplications { get; set; }
        public int TotalUsers { get; set; }
        public IEnumerable<JobListItemViewModel> RecentJobs { get; set; }
    }
}
