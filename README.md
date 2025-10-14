# Online Job Portal

A modern, minimalistic ASP.NET Core MVC application that connects job seekers and administrators. The project implements the functional and non-functional requirements described in the provided SRS, featuring fully fledged job seeker and admin experiences, Identity-powered authentication, resume handling, and responsive UI.

## Features

### Job seekers

### Administrators

### Cross-cutting

## Getting started

### Prerequisites

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

### Updating configuration

### Database management
If you change the data model, add a migration and apply it:
```powershell
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Folder structure highlights

## Known limitations

## Next steps

## JobPortal

## Overview
JobPortal is a web-based job board platform built with ASP.NET Core, Entity Framework Core, and SQL Server. It allows job seekers to search and apply for jobs, and employers/providers to post and manage job listings. The platform also features job alerts, contact forms, and an admin dashboard for site management.

## Features
- User registration and authentication
- Job search and filtering
- Job application submission
- Saved jobs and job alerts
- Contact Us form with preferred contact date (must be today or later)
- Admin dashboard for managing users, jobs, and contact messages
- Email notifications for job alerts
- Responsive UI with Razor views

## How to Run the Project
1. **Clone the repository:**
    ```
    git clone https://github.com/moradiyaaman/JobPortal.git
    cd JobPortal
    ```
2. **Set up the database:**
    - Ensure SQL Server is running and update the connection string in `appsettings.json` if needed.
    - Run migrations:
       ```
       dotnet ef database update
       ```
3. **Run the application:**
    ```
    dotnet run
    ```
    The site will be available at `https://localhost:5001` or `http://localhost:5000` by default.

## Team Members
- **Harshit ce095**: Project lead, backend (ASP.NET Core, EF Core), job alert system, admin features
- **Aman ce**: Frontend (Razor views, CSS), user dashboard, job search UI
- **Neel**: Database design, migrations, deployment (Docker, AWS)

---

For any questions or issues, please contact the project maintainer.
