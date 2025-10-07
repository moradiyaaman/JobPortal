# Online Job Portal

A modern, minimalistic ASP.NET Core MVC application that connects job seekers and administrators. The project implements the functional and non-functional requirements described in the provided SRS, featuring fully fledged job seeker and admin experiences, Identity-powered authentication, resume handling, and responsive UI.

## Features

### Job seekers
- Account registration, login, and profile management with resume upload (PDF/DOC/DOCX, max 5 MB)
- Browse and filter jobs by keyword, country, job type, and recency
- View detailed job descriptions and apply with one click (prevents duplicate applications)
- Track submitted applications with a timeline view
- Contact administrators through the built-in inquiry form

### Administrators
- Secure admin area with dashboard metrics (jobs, applications, users, messages)
- Create, edit, and delete job listings with optional logo upload (PNG/JPG/SVG)
- Inspect applicants per job and download their resumes
- Review every application across the portal and jump to the public job listing
- Respond to contact messages from job seekers

### Cross-cutting
- ASP.NET Core Identity with SQL Server persistence
- Seeded admin account and sample jobs for quick evaluation
- Minimal, responsive UI themed around a light, spacious design across public and admin pages

## Getting started

### Prerequisites
- [.NET SDK 6.0 or later](https://dotnet.microsoft.com/download). The project targets **.NET Core 3.1**, which is out of support; use the .NET 6+ SDK to build and run locally.
- SQL Server LocalDB (installed with Visual Studio) or update the connection string to point to another SQL Server instance.

### Configure and run
1. Restore packages and build:
   ```powershell
   cd "d:\SEM 5\WAD\reading\JobPortal"
   dotnet build
   ```
2. Run the application:
   ```powershell
   dotnet run
   ```
3. Browse to `https://localhost:5001` (or the URL printed in the console).

The first run migrates the database and seeds an admin user plus sample jobs.

### Seeded credentials
- **Admin account:** `admin@jobportal.com`
- **Password:** `Admin@123`

### Updating configuration
- Connection string: `appsettings.json` → `ConnectionStrings:DefaultConnection`
- Resume constraints: `appsettings.json` → `ResumeUpload`

### Database management
If you change the data model, add a migration and apply it:
```powershell
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Folder structure highlights
- `Controllers/` – Public controllers for home, account, jobs, and contact flows
- `Areas/Admin/` – Admin controllers and views for dashboard, jobs, applications, messages
- `Views/Shared/` – Shared layouts for public and admin UI
- `Data/ApplicationDbContext.cs` – EF Core DbContext and relationship configuration
- `Data/SeedData.cs` – Database initialization and seeding logic
- `wwwroot/css/site.css` – Custom minimal UI theme

## Known limitations
- Target framework is netcoreapp3.1 (out of support). Consider upgrading to .NET 8 for long-term maintenance.
- No automated tests are currently included; add unit/integration tests before production deployment.

## Next steps
- Upgrade the project to .NET 8 and update dependencies
- Integrate email notifications for new applications or inquiries
- Add pagination to job listings for large datasets
