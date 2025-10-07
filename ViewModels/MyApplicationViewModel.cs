using System;

namespace JobPortal.ViewModels
{
    public class MyApplicationViewModel
    {
        public int JobId { get; set; }
        public string JobTitle { get; set; }
        public string CompanyName { get; set; }
        public string Location { get; set; }
        public DateTime AppliedAt { get; set; }
        public string Status { get; set; }
    }
}
