using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JobPortal.ViewModels
{
    public class RegisterViewModel : IValidatableObject
    {
        [Required]
        [Display(Name = "Account type")]
        public string AccountType { get; set; } = "Seeker";

        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Full name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Mobile number")]
        public string Mobile { get; set; }

        [Required]
        public string Country { get; set; }

        [Display(Name = "Address")]
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Password and confirmation do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; }

        [Display(Name = "Company name")]
        public string CompanyName { get; set; }

        [Display(Name = "Company website")]
        [Url]
        public string CompanyWebsite { get; set; }

        [Display(Name = "Company location")]
        public string CompanyLocation { get; set; }

        [Display(Name = "Company description")]
        [MaxLength(500)]
        public string CompanyDescription { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AccountType?.Equals("Provider", System.StringComparison.OrdinalIgnoreCase) == true)
            {
                if (string.IsNullOrWhiteSpace(CompanyName))
                {
                    yield return new ValidationResult("Company name is required for provider accounts.", new[] { nameof(CompanyName) });
                }

                if (string.IsNullOrWhiteSpace(CompanyLocation))
                {
                    yield return new ValidationResult("Company location is required for provider accounts.", new[] { nameof(CompanyLocation) });
                }

                if (string.IsNullOrWhiteSpace(CompanyDescription))
                {
                    yield return new ValidationResult("Please provide a short company description.", new[] { nameof(CompanyDescription) });
                }
            }
        }
    }
}
