using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JobPortal.Configuration;
using JobPortal.Data;
using JobPortal.Models;
using JobPortal.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JobPortal
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireDigit = true;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
            });

            // Increase multipart/form-data limits (affects model binding)
            services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
                o.ValueLengthLimit = int.MaxValue;
                o.MultipartHeadersLengthLimit = int.MaxValue;
            });

            // Ensure large uploads work in both hosting modes
            services.Configure<IISServerOptions>(o =>
            {
                o.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
                // Some components in IIS Express may still use sync IO in .NET Core 3.1
                o.AllowSynchronousIO = true;
            });

            services.Configure<KestrelServerOptions>(o =>
            {
                o.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
            });

            services.AddControllersWithViews();

            services.Configure<EmailSettings>(Configuration.GetSection("EmailSettings"));
            services.AddTransient<IEmailService, SmtpEmailService>();
            services.AddHostedService<JobAlertDispatcher>();

            // ATS scorer
            services.AddTransient<IAtsScorer, SimpleAtsScorer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Ensure the target DB for the active profile is migrated at app start
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
                db.Database.Migrate();

                try
                {
                    var conn = db.Database.GetDbConnection();
                    var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(conn.ConnectionString);
                    var applied = db.Database.GetAppliedMigrations().ToList();
                    logger.LogInformation("Connected to SQL Server {Server} / DB {Database}. Applied migrations: {Count}. Last: {Last}", builder.DataSource, builder.InitialCatalog, applied.Count, applied.LastOrDefault());
                }
                catch (Exception ex)
                {
                    var applied = db.Database.GetAppliedMigrations().ToList();
                    var last = applied.LastOrDefault();
                    var count = applied.Count;
                    var msg = $"Applied migrations: {count}. Last: {last}";
                    var cs = db.Database.GetDbConnection()?.ConnectionString ?? "<unknown>";
                    var redacted = cs;
                    logger.LogWarning(ex, "Startup DB diagnostics failed. {Msg}. ConnString: {Conn}", msg, redacted);
                }
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
