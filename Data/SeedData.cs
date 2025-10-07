using System;
using System.Linq;
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

            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await context.Database.MigrateAsync();
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }

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

            const string adminEmail = "admin@jobportal.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    FullName = "Portal Administrator",
                    EmailConfirmed = true,
                    IsAdmin = true,
                    Country = "Remote"
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, adminRole);
                }
            }
            else if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }

            const string providerEmail = "provider@jobportal.com";
            var providerUser = await userManager.FindByEmailAsync(providerEmail);
            if (providerUser == null)
            {
                providerUser = new ApplicationUser
                {
                    UserName = "provider",
                    Email = providerEmail,
                    FullName = "LaunchFlow HR Team",
                    EmailConfirmed = true,
                    Country = "USA",
                    IsProvider = true,
                    CompanyName = "LaunchFlow",
                    CompanyWebsite = "https://launchflow.example",
                    CompanyDescription = "Growth-focused SaaS accelerator helping startups launch and scale.",
                    CompanyLocation = "Remote"
                };

                var providerResult = await userManager.CreateAsync(providerUser, "Provider@123");
                if (providerResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(providerUser, providerRole);
                }
            }
            else if (!await userManager.IsInRoleAsync(providerUser, providerRole))
            {
                await userManager.AddToRoleAsync(providerUser, providerRole);
            }

            if (!context.Jobs.Any())
            {
                var providerId = providerUser?.Id;
                context.Jobs.AddRange(
                    new Job
                    {
                        Title = "Senior UX Designer",
                        CompanyName = "Pixelcraft Studio",
                        Location = "San Francisco, CA",
                        Country = "USA",
                        JobType = "Full-time",
                        Salary = "$120k - $140k",
                        Description = "Lead end-to-end product design initiatives, working closely with cross-functional teams to shape delightful user experiences.",
                        Qualifications = "5+ years in UX/UI design, strong portfolio, proficiency in Figma and prototyping tools.",
                        Skills = "UX Research, Wireframing, Prototyping, Design Systems",
                        PostedAt = DateTime.UtcNow.AddDays(-5),
                        ProviderId = providerId,
                        CompanyWebsite = providerUser?.CompanyWebsite,
                        ProviderDisplayName = providerUser?.CompanyName,
                        ProviderSummary = providerUser?.CompanyDescription,
                        CompanyLogoPath = providerUser?.CompanyLogoPath
                    },
                    new Job
                    {
                        Title = "Full Stack Developer",
                        CompanyName = "CloudForge",
                        Location = "Remote",
                        Country = "Global",
                        JobType = "Remote",
                        Salary = "$90k - $110k",
                        Description = "Build and maintain scalable web apps with a focus on performance and maintainability.",
                        Qualifications = "Experience with ASP.NET Core, React, SQL Server",
                        Skills = "C#, ASP.NET Core, React, SQL",
                        PostedAt = DateTime.UtcNow.AddDays(-2),
                        ProviderId = providerId,
                        CompanyWebsite = providerUser?.CompanyWebsite,
                        ProviderDisplayName = providerUser?.CompanyName,
                        ProviderSummary = providerUser?.CompanyDescription,
                        CompanyLogoPath = providerUser?.CompanyLogoPath
                    },
                    new Job
                    {
                        Title = "Product Marketing Manager",
                        CompanyName = "LaunchFlow",
                        Location = "Berlin, Germany",
                        Country = "Germany",
                        JobType = "Hybrid",
                        Salary = "€70k - €85k",
                        Description = "Own go-to-market strategies, craft compelling narratives, and enable sales teams with tailored messaging.",
                        Qualifications = "4+ years in product marketing, SaaS experience preferred.",
                        Skills = "Positioning, Messaging, Campaigns, Analytics",
                        PostedAt = DateTime.UtcNow.AddDays(-10),
                        ProviderId = providerId,
                        CompanyWebsite = providerUser?.CompanyWebsite,
                        ProviderDisplayName = providerUser?.CompanyName,
                        ProviderSummary = providerUser?.CompanyDescription,
                        CompanyLogoPath = providerUser?.CompanyLogoPath
                    }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
