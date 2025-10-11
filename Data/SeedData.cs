using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JobPortal.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobPortal.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var scopedServices = scope.ServiceProvider;

            var context = scopedServices.GetRequiredService<ApplicationDbContext>();

            await context.Database.MigrateAsync();

            var roleManager = scopedServices.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scopedServices.GetRequiredService<UserManager<ApplicationUser>>();

            const string adminRole = "Admin";
            const string providerRole = "Provider";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            if (!await roleManager.RoleExistsAsync(providerRole))
            {
                await roleManager.CreateAsync(new IdentityRole(providerRole));
            }

            await SeedAdminsAsync(userManager, adminRole);
            await SeedJobSeekersAsync(userManager);
            await SeedProvidersAndJobsAsync(context, userManager, providerRole);
        }

        private static async Task SeedAdminsAsync(UserManager<ApplicationUser> userManager, string adminRole)
        {
            var admins = new List<BasicUserSeed>
            {
                new BasicUserSeed("admin.neha", "Neha Sharma", "neha.sharma@jobportal.in"),
                new BasicUserSeed("admin.rajiv", "Rajiv Menon", "rajiv.menon@jobportal.in"),
                new BasicUserSeed("admin.kavita", "Kavita Iyer", "kavita.iyer@jobportal.in"),
                new BasicUserSeed("admin.anil", "Anil Gupta", "anil.gupta@jobportal.in"),
                new BasicUserSeed("admin.priya", "Priya Nair", "priya.nair@jobportal.in")
            };

            foreach (var seed in admins)
            {
                var user = await userManager.FindByEmailAsync(seed.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = seed.UserName,
                        Email = seed.Email,
                        FullName = seed.FullName,
                        EmailConfirmed = true,
                        Country = "India",
                        IsAdmin = true
                    };

                    var result = await userManager.CreateAsync(user, "Password@123");
                    if (!result.Succeeded)
                    {
                        continue;
                    }
                }

                if (!user.IsAdmin)
                {
                    user.IsAdmin = true;
                    await userManager.UpdateAsync(user);
                }

                if (!await userManager.IsInRoleAsync(user, adminRole))
                {
                    await userManager.AddToRoleAsync(user, adminRole);
                }
            }
        }

        private static async Task SeedJobSeekersAsync(UserManager<ApplicationUser> userManager)
        {
            var seekers = new List<BasicUserSeed>
            {
                new BasicUserSeed("aarav.sharma", "Aarav Sharma", "aarav.sharma@seekers.in"),
                new BasicUserSeed("vihaan.patel", "Vihaan Patel", "vihaan.patel@seekers.in"),
                new BasicUserSeed("aditya.singh", "Aditya Singh", "aditya.singh@seekers.in"),
                new BasicUserSeed("aria.iyer", "Aria Iyer", "aria.iyer@seekers.in"),
                new BasicUserSeed("kiran.reddy", "Kiran Reddy", "kiran.reddy@seekers.in"),
                new BasicUserSeed("meera.nair", "Meera Nair", "meera.nair@seekers.in"),
                new BasicUserSeed("rohan.das", "Rohan Das", "rohan.das@seekers.in"),
                new BasicUserSeed("sanya.kapoor", "Sanya Kapoor", "sanya.kapoor@seekers.in"),
                new BasicUserSeed("dev.mehta", "Dev Mehta", "dev.mehta@seekers.in"),
                new BasicUserSeed("isha.gupta", "Isha Gupta", "isha.gupta@seekers.in"),
                new BasicUserSeed("neeraj.kulkarni", "Neeraj Kulkarni", "neeraj.kulkarni@seekers.in"),
                new BasicUserSeed("ria.banerjee", "Ria Banerjee", "ria.banerjee@seekers.in"),
                new BasicUserSeed("tanishq.bose", "Tanishq Bose", "tanishq.bose@seekers.in"),
                new BasicUserSeed("amara.pillai", "Amara Pillai", "amara.pillai@seekers.in"),
                new BasicUserSeed("ishaan.rao", "Ishaan Rao", "ishaan.rao@seekers.in"),
                new BasicUserSeed("kavya.menon", "Kavya Menon", "kavya.menon@seekers.in"),
                new BasicUserSeed("arnav.joshi", "Arnav Joshi", "arnav.joshi@seekers.in"),
                new BasicUserSeed("diya.chawla", "Diya Chawla", "diya.chawla@seekers.in"),
                new BasicUserSeed("samar.varma", "Samar Varma", "samar.varma@seekers.in"),
                new BasicUserSeed("anika.desai", "Anika Desai", "anika.desai@seekers.in"),
                new BasicUserSeed("harsh.venkatesh", "Harsh Venkatesh", "harsh.venkatesh@seekers.in"),
                new BasicUserSeed("nidhi.saxena", "Nidhi Saxena", "nidhi.saxena@seekers.in"),
                new BasicUserSeed("parth.srivastava", "Parth Srivastava", "parth.srivastava@seekers.in"),
                new BasicUserSeed("trisha.fernandes", "Trisha Fernandes", "trisha.fernandes@seekers.in"),
                new BasicUserSeed("yuvraj.ahuja", "Yuvraj Ahuja", "yuvraj.ahuja@seekers.in"),
                new BasicUserSeed("tanvi.sethi", "Tanvi Sethi", "tanvi.sethi@seekers.in"),
                new BasicUserSeed("kunal.bhatia", "Kunal Bhatia", "kunal.bhatia@seekers.in"),
                new BasicUserSeed("malini.krishnan", "Malini Krishnan", "malini.krishnan@seekers.in"),
                new BasicUserSeed("pranav.shetty", "Pranav Shetty", "pranav.shetty@seekers.in"),
                new BasicUserSeed("aditi.kulshreshtha", "Aditi Kulshreshtha", "aditi.kulshreshtha@seekers.in")
            };

            for (var i = 0; i < seekers.Count; i++)
            {
                var seed = seekers[i];
                var user = await userManager.FindByEmailAsync(seed.Email);
                if (user != null)
                {
                    continue;
                }

                var newUser = new ApplicationUser
                {
                    UserName = seed.UserName,
                    Email = seed.Email,
                    FullName = seed.FullName,
                    EmailConfirmed = true,
                    Country = "India",
                    Headline = "Indian professional exploring new opportunities",
                    CreatedAt = DateTime.UtcNow.AddDays(-(i + 5))
                };

                await userManager.CreateAsync(newUser, "Password@123");
            }
        }

        private static async Task SeedProvidersAndJobsAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, string providerRole)
        {
            var providers = new List<ProviderSeed>
            {
                new ProviderSeed("arjun.mehta", "Arjun Mehta", "arjun.mehta@providers.in", "Mehta Tech Solutions", "Product engineering studio supporting Indian retail innovation.", "Mumbai", "Maharashtra"),
                new ProviderSeed("priya.verma", "Priya Verma", "priya.verma@providers.in", "Verma Digital Services", "UX and mobile lab building engaging consumer apps.", "Bengaluru", "Karnataka"),
                new ProviderSeed("rohan.patel", "Rohan Patel", "rohan.patel@providers.in", "Patel Analytics", "Data science consultancy for BFSI transformations.", "Ahmedabad", "Gujarat"),
                new ProviderSeed("sneha.reddy", "Sneha Reddy", "sneha.reddy@providers.in", "Hyderabad CloudWorks", "Azure migration specialists serving enterprises across India.", "Hyderabad", "Telangana"),
                new ProviderSeed("vikram.singh", "Vikram Singh", "vikram.singh@providers.in", "Singh CyberSec Labs", "Cybersecurity product house protecting public sector deployments.", "New Delhi", "Delhi"),
                new ProviderSeed("ananya.nair", "Ananya Nair", "ananya.nair@providers.in", "Nair HealthTech", "Digital transformation partner for hospitals and clinics.", "Kochi", "Kerala"),
                new ProviderSeed("sahil.khanna", "Sahil Khanna", "sahil.khanna@providers.in", "Khanna AI Research", "AI/ML innovation hub for manufacturing analytics.", "Gurugram", "Haryana"),
                new ProviderSeed("isha.desai", "Isha Desai", "isha.desai@providers.in", "Desai Cloud Ventures", "Multi-cloud DevOps and platform engineering experts.", "Pune", "Maharashtra"),
                new ProviderSeed("manish.rao", "Manish Rao", "manish.rao@providers.in", "Rao Automotive Systems", "Embedded software specialists for mobility OEMs.", "Chennai", "Tamil Nadu"),
                new ProviderSeed("nandini.bose", "Nandini Bose", "nandini.bose@providers.in", "Bose FinServe Tech", "Financial services implementation agency for NBFCs.", "Kolkata", "West Bengal"),
                new ProviderSeed("kabir.jain", "Kabir Jain", "kabir.jain@providers.in", "Jain Smart Mobility", "EV mobility platform builders for smart cities.", "Jaipur", "Rajasthan"),
                new ProviderSeed("lavanya.menon", "Lavanya Menon", "lavanya.menon@providers.in", "Menon Retail Labs", "Omnichannel retail solutions consultancy.", "Thiruvananthapuram", "Kerala"),
                new ProviderSeed("devansh.bhatt", "Devansh Bhatt", "devansh.bhatt@providers.in", "Bhatt AgroTech", "Agri-tech analytics and sensor integrations for cooperatives.", "Indore", "Madhya Pradesh"),
                new ProviderSeed("tanya.ahmed", "Tanya Ahmed", "tanya.ahmed@providers.in", "Ahmed SmartInfra", "IoT infrastructure consultancy for smart campuses.", "Noida", "Uttar Pradesh"),
                new ProviderSeed("yash.swamy", "Yash Swamy", "yash.swamy@providers.in", "Swamy Learning Systems", "EdTech product studio powering digital classrooms.", "Bengaluru", "Karnataka")
            };

            var jobTemplates = new List<JobTemplate>
            {
                new JobTemplate("Senior Software Engineer", "Design and build scalable .NET services for high-growth platforms.", "B.E/B.Tech in Computer Science with 6+ years in .NET ecosystems.", "C#, ASP.NET Core, Microservices, Azure, SQL", "Full-time", "₹18L - ₹24L"),
                new JobTemplate("Frontend Engineer", "Craft intuitive interfaces and design systems for large consumer apps.", "4+ years with modern JavaScript frameworks.", "React, TypeScript, State Management, UI Testing", "Hybrid", "₹14L - ₹20L"),
                new JobTemplate("Data Analyst", "Translate data into insights for CXO dashboards and business teams.", "3+ years in analytics or BI roles.", "SQL, Power BI, Python, Storytelling", "Full-time", "₹10L - ₹14L"),
                new JobTemplate("DevOps Engineer", "Automate infrastructure and CI/CD pipelines across multi-cloud setups.", "Experience managing production workloads on Azure/AWS.", "Azure DevOps, Terraform, Kubernetes, Monitoring", "Full-time", "₹16L - ₹21L"),
                new JobTemplate("Product Manager", "Drive product discovery and delivery with cross-functional squads.", "5+ years in product roles with SaaS exposure.", "Roadmapping, Stakeholder Management, Analytics", "Hybrid", "₹20L - ₹26L"),
                new JobTemplate("QA Automation Lead", "Own automation strategy and frameworks for mission-critical releases.", "7+ years in QA with leadership experience.", "Selenium, Playwright, API Testing, CI/CD", "Full-time", "₹15L - ₹19L")
            };

            bool jobsAdded = false;

            for (var index = 0; index < providers.Count; index++)
            {
                var seed = providers[index];
                var user = await userManager.FindByEmailAsync(seed.Email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        UserName = seed.UserName,
                        Email = seed.Email,
                        FullName = seed.FullName,
                        EmailConfirmed = true,
                        Country = "India",
                        IsProvider = true,
                        CompanyName = seed.CompanyName,
                        CompanyDescription = seed.CompanyDescription,
                        CompanyLocation = $"{seed.City}, {seed.State}",
                        CompanyWebsite = $"https://{Slugify(seed.CompanyName)}.in"
                    };

                    var result = await userManager.CreateAsync(user, "Password@123");
                    if (!result.Succeeded)
                    {
                        continue;
                    }
                }

                bool providerUpdated = false;

                if (!user.IsProvider)
                {
                    user.IsProvider = true;
                    providerUpdated = true;
                }

                if (!string.Equals(user.CompanyName, seed.CompanyName, StringComparison.Ordinal))
                {
                    user.CompanyName = seed.CompanyName;
                    providerUpdated = true;
                }

                if (!string.Equals(user.CompanyDescription, seed.CompanyDescription, StringComparison.Ordinal))
                {
                    user.CompanyDescription = seed.CompanyDescription;
                    providerUpdated = true;
                }

                var expectedLocation = $"{seed.City}, {seed.State}";
                if (!string.Equals(user.CompanyLocation, expectedLocation, StringComparison.Ordinal))
                {
                    user.CompanyLocation = expectedLocation;
                    providerUpdated = true;
                }

                var expectedWebsite = $"https://{Slugify(seed.CompanyName)}.in";
                if (!string.Equals(user.CompanyWebsite, expectedWebsite, StringComparison.OrdinalIgnoreCase))
                {
                    user.CompanyWebsite = expectedWebsite;
                    providerUpdated = true;
                }

                if (providerUpdated)
                {
                    await userManager.UpdateAsync(user);
                }

                if (!await userManager.IsInRoleAsync(user, providerRole))
                {
                    await userManager.AddToRoleAsync(user, providerRole);
                }

                var existingJobs = await context.Jobs
                    .Where(j => j.ProviderId == user.Id)
                    .Select(j => j.Title)
                    .ToListAsync();

                for (var jobIndex = existingJobs.Count; jobIndex < 3; jobIndex++)
                {
                    var template = jobTemplates[(index * 3 + jobIndex) % jobTemplates.Count];
                    var job = new Job
                    {
                        Title = template.Title,
                        Description = template.Description,
                        Qualifications = template.Qualifications,
                        Skills = template.Skills,
                        JobType = template.JobType,
                        Salary = template.Salary,
                        Location = $"{seed.City}, {seed.State}",
                        Country = "India",
                        IsActive = true,
                        ProviderId = user.Id,
                        CompanyName = user.CompanyName ?? user.FullName,
                        CompanyWebsite = user.CompanyWebsite,
                        CompanyLogoPath = user.CompanyLogoPath,
                        ProviderDisplayName = user.CompanyName ?? user.FullName,
                        ProviderSummary = user.CompanyDescription,
                        PostedAt = DateTime.UtcNow.AddDays(-((index * 3) + jobIndex + 1))
                    };

                    context.Jobs.Add(job);
                    jobsAdded = true;
                }
            }

            if (jobsAdded)
            {
                await context.SaveChangesAsync();
            }
        }

        private static string Slugify(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "company";
            }

            var sb = new StringBuilder();
            foreach (var ch in value.ToLowerInvariant())
            {
                if (char.IsLetterOrDigit(ch))
                {
                    sb.Append(ch);
                }
            }

            return sb.Length == 0 ? "company" : sb.ToString();
        }

        private sealed class BasicUserSeed
        {
            public BasicUserSeed(string userName, string fullName, string email)
            {
                UserName = userName;
                FullName = fullName;
                Email = email;
            }

            public string UserName { get; }
            public string FullName { get; }
            public string Email { get; }
        }

        private sealed class ProviderSeed
        {
            public ProviderSeed(string userName, string fullName, string email, string companyName, string companyDescription, string city, string state)
            {
                UserName = userName;
                FullName = fullName;
                Email = email;
                CompanyName = companyName;
                CompanyDescription = companyDescription;
                City = city;
                State = state;
            }

            public string UserName { get; }
            public string FullName { get; }
            public string Email { get; }
            public string CompanyName { get; }
            public string CompanyDescription { get; }
            public string City { get; }
            public string State { get; }
        }

        private sealed class JobTemplate
        {
            public JobTemplate(string title, string description, string qualifications, string skills, string jobType, string salary)
            {
                Title = title;
                Description = description;
                Qualifications = qualifications;
                Skills = skills;
                JobType = jobType;
                Salary = salary;
            }

            public string Title { get; }
            public string Description { get; }
            public string Qualifications { get; }
            public string Skills { get; }
            public string JobType { get; }
            public string Salary { get; }
        }
    }
}
