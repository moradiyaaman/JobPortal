# JobPortal

Modern job board built with ASP.NET Core MVC and Entity Framework Core for connecting job seekers, employers, and administrators on a single platform.

## Table of Contents
- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Core Features](#core-features)
- [Architecture Highlights](#architecture-highlights)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Configuration](#configuration)
  - [Run the App](#run-the-app)
  - [Seeded Sign-in Accounts](#seeded-sign-in-accounts)
- [Database Migrations](#database-migrations)
- [Project Structure](#project-structure)
- [Quality & Testing](#quality--testing)
- [Team](#team)

## Overview
JobPortal delivers an end-to-end hiring workflow: candidates explore and apply for curated roles, providers publish openings, and administrators oversee platform health. The application ships with sample data, role-based authorization, and a responsive UI so you can demonstrate functionality immediately after setup.

## Tech Stack
- ASP.NET Core 3.1 MVC (C#)
- Entity Framework Core with SQL Server
- ASP.NET Core Identity for authentication & authorization
- Razor Views with Tailwind-inspired utility CSS
- Background services for email dispatching (job alerts)
- Docker-ready configuration & cloud-friendly logging

## Core Features
### Job Seekers
- Register, sign in, and manage personal profiles with resume uploads.
- Search and filter active jobs by keyword, country, and job type.
- Submit applications with optional cover letters and track status over time.
- Save interesting roles and configure job alerts tailored to personal criteria.
- Reach out via the Contact Us form, selecting a preferred contact date that must be today or later.

### Providers & Administrators
- Provider personas publish, edit, and archive job listings with company branding.
- Admin area exposes dashboards for jobs, applications, alerts, and contact messages.
- Audit incoming contact messages with visibility into request details and preferred follow-up dates.
- Manage high-volume candidate pipelines with exportable application data and resume access.

### Platform-wide
- Role-based navigation and permissions enforced through ASP.NET Core Identity.
- Scheduled background service emails matching alerts to new jobs every hour.
- Centralized configuration for uploads, email (SMTP), and environment-specific overrides.
- Fully responsive layout optimized for desktop and tablet experiences.

## Architecture Highlights
- `Controllers/` and `Areas/Admin/Controllers/` implement MVC entry points for public and admin experiences.
- `Data/ApplicationDbContext.cs` configures EF Core entities, relationships, and indexes.
- `Services/JobAlertDispatcher.cs` runs as a hosted background service to deliver job alert emails.
- `ViewModels/` encapsulate strongly typed data passed to Razor views.
- `SeedData.cs` automatically provisions roles, admin accounts, providers, seekers, and sample jobs.

## Getting Started

### Prerequisites
- [.NET 6 SDK](https://dotnet.microsoft.com/download) or newer (SDK only; app targets netcoreapp3.1).
- SQL Server LocalDB (bundled with Visual Studio) or another SQL Server instance.
- (Optional) Node.js/NPM if you plan to rebuild static assets.

### Configuration
1. Clone the repository:
    ```powershell
    git clone https://github.com/moradiyaaman/JobPortal.git
    cd JobPortal
    ```
2. Update `appsettings.json` (and environment-specific variants) with your SQL Server connection string and SMTP settings.
3. Restore dependencies:
    ```powershell
    dotnet restore
    ```

### Run the App
Apply database migrations and start the site:
```powershell
dotnet ef database update
dotnet run
```
Navigate to `https://localhost:5001` (HTTPS) or `http://localhost:5000` (HTTP).

### Seeded Sign-in Accounts
The seeding routine creates multiple accounts with the password `Password@123`.
- **Administrators**: e.g., `neha.sharma@jobportal.in`, `rajiv.menon@jobportal.in`
- **Providers**: e.g., `arjun.mehta@providers.in`, `priya.verma@providers.in`
- **Job Seekers**: e.g., `aarav.sharma@seekers.in`, `meera.nair@seekers.in`

Use the admin role to explore the dashboard and provider accounts to publish jobs. Job seeker accounts showcase the application flow.

## Database Migrations
When updating the data model:
```powershell
dotnet ef migrations add <MigrationName>
dotnet ef database update
```
For production environments, use generated migrations with your deployment pipeline (Docker, CI/CD, or manual SQL execution).

## Project Structure
- `Areas/Admin/` – admin-specific controllers, views, and layouts.
- `Controllers/` – public-facing MVC controllers (jobs, account, contact, etc.).
- `Models/` – entity definitions including `ContactMessage` with preferred contact date validation.
- `Services/` – background services and abstractions (`JobAlertDispatcher`, `IEmailService`).
- `ViewModels/` – data transfer objects consumed by Razor views.
- `wwwroot/` – static assets (CSS, JavaScript, vendor libraries, uploads).

## Quality & Testing
- Input validation enforced with data annotations and server-side ModelState checks.
- Preferred contact dates are validated to ensure they are not in the past.
- Background services and database operations use cancellation tokens for graceful shutdown.
- Automated test suite is not yet included; add unit/integration tests before production use.

## Team
| Team Member | Responsibilities |
|-------------|------------------|
| Harshit (ce095) | Solution architecture, backend APIs, job alert pipeline, DevOps automation |
| Aman (ce) | Frontend UX, Razor view components, responsive styling, accessibility review |
| Neel | Database schema design, EF Core migrations, infrastructure (Docker, AWS deployment) |

For questions or collaboration ideas, feel free to open an issue or reach out to the maintainers.
