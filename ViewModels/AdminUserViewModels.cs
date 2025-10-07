using System;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.ViewModels
{
    public class AdminUserListItemViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsProvider { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime CreatedAt { get; set; }
        public int JobsPosted { get; set; }
        public int ApplicationsSubmitted { get; set; }
    }

    public class AdminUserDetailsViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Mobile { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public string Headline { get; set; }
        public string Summary { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsProvider { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsLockedOut { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CompanyName { get; set; }
        public string CompanyWebsite { get; set; }
        public string CompanyLocation { get; set; }
        public string CompanyDescription { get; set; }
        public int JobsPosted { get; set; }
        public int ApplicationsSubmitted { get; set; }
    }

    public class AdminCreateAdminViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password confirmation does not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }
    }
}
