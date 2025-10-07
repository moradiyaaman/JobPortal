using JobPortal.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<SavedJob> SavedJobs { get; set; }
        public DbSet<JobAlertSubscription> JobAlertSubscriptions { get; set; }
        public DbSet<ContentPage> ContentPages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<JobApplication>()
                .HasIndex(a => new { a.JobId, a.ApplicantId })
                .IsUnique();

            builder.Entity<SavedJob>()
                .HasIndex(s => new { s.JobId, s.UserId })
                .IsUnique();

            builder.Entity<JobAlertSubscription>()
                .HasIndex(a => new { a.UserId, a.Keyword, a.Country, a.JobType })
                .IsUnique();

            builder.Entity<ContentPage>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            builder.Entity<Job>()
                .HasMany(j => j.Applications)
                .WithOne(a => a.Job)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Applications)
                .WithOne(a => a.Applicant)
                .HasForeignKey(a => a.ApplicantId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.JobsPosted)
                .WithOne(j => j.Provider)
                .HasForeignKey(j => j.ProviderId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.SavedJobs)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.JobAlerts)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
