# M.I.B. Library Management System (MPOnline)

## Project Title & System Overview

The **M.I.B. Library Management System** is a futuristic, M.I.B.-themed library and archive platform built with **ASP.NET Core MVC** and **Entity Framework Core**. The application presents the library as a secure command-and-control resource network for agents, staff, and administrators, with a dark tactical user interface, role-aware access, searchable directories, borrowing workflows, and a telemetry-style dashboard.

The project is designed around a few core operational ideas:

- **Centralized resource control** for books and classified publications.
- **Agent and staff directories** for students and librarians.
- **Borrowing and return workflows** with live availability tracking.
- **Identity-based authentication** for secure access to sensitive actions.
- **A telemetry dashboard** that summarizes the live state of the system in a mission-control style view.

This repository currently runs with a seeded development configuration and supports both **EF Core In-Memory** execution and **SQL Server** persistence through the same `ApplicationDbContext`.

## Core Features

### Telemetry & Stats Dashboard

The dashboard acts as the system's command panel. It aggregates key operational metrics from the database and presents them as live status cards and recent activity lists.

Primary dashboard indicators include:

- **Active Borrowings**: borrow records where `IsReturned == false`.
- **Overdue Alerts**: active borrowings whose `DueDate` is earlier than the current time.
- **Sector Logs / Recent Transactions**: the five newest borrow activity records, including book title, agent name, card ID, borrow date, due date, and return state.
- **Entity Totals**: total books, publications, students, and librarians currently stored in the database.

The dashboard code is defensive by design. If the borrow record set is missing or the query path fails, the controller falls back to zeroed metrics and empty transaction lists instead of crashing the page.

### Inventory Management

The system manages two primary inventory streams:

- **Books**: structured assets with title, author, ISBN, publisher, publish date, and copy counts.
- **Classified Publications**: a separate archive for newspapers and magazines, tracked through a publication type enumeration.

Books support borrowing and returning, while publications support indexed browsing and CRUD-style lifecycle management for authorized staff.

### Directory Management

The application includes two personnel directories:

- **Students**, themed as **agents**.
- **Librarians**, themed as **staff**.

Each directory supports:

- searchable listing views,
- ordered pagination,
- card-like detail presentation in the UI,
- identity-safe browsing through MVC views.

The student directory is ordered by `StudentCardId`, while the librarian directory is ordered by last name and first name.

### Safe Pagination & Search Filters

The list pages are designed for controlled data access and predictable paging behavior.

Observed controller patterns in the repository:

- **Books**: search by title, author, or ISBN; page size of 5.
- **Students**: search by first name, last name, or card ID; page size of 10.
- **Librarians**: search by first name, last name, email, or employee ID; page size of 5.
- **Publications**: search by title or publisher; page size of 10.

Pagination is built with `Skip(...)` and `Take(...)`, while the Razor views preserve the active search term across page changes. The student and publication views also clamp the current page so navigation cannot go below page 1 or beyond the last page.

### Identity Authentication

The project uses **ASP.NET Core Identity** for authentication, with a custom login flow exposed through `AccountController`.

Implemented roles seeded in the application:

- `Admin`
- `Librarian`
- `Student`

Authentication details:

- Login accepts either email or username.
- Logout is handled through a POST action.
- Privileged publication management actions are role-restricted to `Admin` and `Librarian`.
- The application cookie paths are customized to route sign-in and sign-out through the `/Account` controller.

## Tech Stack & Dependencies

### Framework

- **.NET 8**
- **ASP.NET Core MVC**
- **ASP.NET Core Identity**

### Data Access

- **Entity Framework Core 8**
- **ApplicationDbContext** as the primary database context
- **SQL Server** support via `Microsoft.EntityFrameworkCore.SqlServer`
- **In-Memory database** support via `Microsoft.EntityFrameworkCore.InMemory`

### UI / Styling

- **Custom dark futuristic CSS** in `wwwroot/css/site.css`
- **Bootstrap 5 assets** available through the project’s static web assets pipeline
- **Font Awesome iconography** used throughout the Razor views for the M.I.B. visual language

### Project Packages

The project file includes the following main dependencies:

- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.AspNetCore.Identity.UI`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Microsoft.EntityFrameworkCore.InMemory`
- `Microsoft.EntityFrameworkCore.Tools`
- `Microsoft.VisualStudio.Web.CodeGeneration.Design`

### Runtime Configuration

The current appsettings configuration is development-friendly:

- `DefaultConnection` points to a local SQL Server LocalDB database named `LibraryDb`.
- `UseInMemoryDatabase` is set to `true`, which means the application starts with in-memory storage unless the setting is changed.

## Database Models & Entity Schema

This project uses a small set of core models to represent the operational state of the library.

### `Book`

Represents a borrowable library asset.

Key fields:

- `Id`: primary key.
- `Title`: required, max length 200.
- `Author`: required, max length 100.
- `ISBN`: required, max length 50.
- `Publisher`: optional text field with a default empty string.
- `PublishDate`: publication date displayed as a date-only value.
- `TotalCopies`: required inventory total.
- `AvailableCopies`: required live availability count.

Relationship:

- `BorrowRecords`: navigation collection used to trace borrow history for each book.

Operational meaning:

- `TotalCopies` reflects the asset stock.
- `AvailableCopies` is decremented on borrow and incremented on return.

### `Publication`

Represents a classified periodical entry in the archive.

Key fields:

- `Id`: primary key.
- `Title`: required title.
- `Author`: optional author field.
- `Type`: required `PublicationType` value.
- `Publisher`: issuing source.
- `IssueNumber`: issue identifier string.
- `ReleaseDate`: release date.
- `Frequency`: optional cadence label such as Daily, Weekly, or Monthly.

Supported publication categories:

- `Newspaper`
- `Magazine`

### `StudentModel`

Represents an agent directory record.

Key fields:

- `Id`: primary key.
- `FirstName`: required.
- `LastName`: required.
- `Email`: required and validated as an email address.
- `PhoneNumber`: optional contact number.
- `StudentCardId`: required agent identifier.
- `EnrollmentDate`: required date.
- `IsActive`: active directory status.

Computed helper:

- `FullName`: concatenation of first and last name.

Operational meaning:

- Only active students are shown in the borrow workflow selection list.

### `LibrarianModel`

Represents a staff directory record.

Key fields:

- `Id`: primary key.
- `FirstName`: required.
- `LastName`: required.
- `Email`: required and validated as an email address.
- `PhoneNumber`: optional contact number.
- `EmployeeId`: required staff identifier.
- `HireDate`: required date.
- `IsActive`: active staff status.

Computed helper:

- `FullName`: concatenation of first and last name.

Operational meaning:

- The librarian directory is searchable and paginated like the student directory.

### `BorrowRecord`

Represents a single checkout lifecycle entry.

Key fields:

- `Id`: primary key.
- `BookId`: foreign key to `Book`.
- `StudentId`: foreign key to `StudentModel`.
- `BorrowDate`: checkout date.
- `DueDate`: expected return date, defaulting to 14 days after borrow.
- `ReturnDate`: nullable actual return date.
- `IsReturned`: return flag.

Navigation properties:

- `Book`: linked book asset.
- `Student`: linked agent record.

Operational meaning:

- Borrowing removes one available copy from the linked book.
- Returning marks the record complete and restores stock.

### `DashboardViewModel`

This is a **view model**, not a persisted entity. It aggregates dashboard totals and recent activity.

Fields:

- `TotalBooks`
- `TotalPublications`
- `TotalStudents`
- `TotalLibrarians`
- `ActiveBorrowings`
- `OverdueBorrowings`
- `TotalTransactions`
- `TotalBorrowings`
- `RecentTransactions`
- `RecentBorrowings`

Supporting model:

- `RecentTransactionViewModel` carries book title, agent name, card ID, borrow date, due date, and return state for the latest entries.

Important note:

- Because this class is not mapped as a database entity, it exists only for presentation in the dashboard view.

## System Setup & Execution Guide

### Prerequisites

- **.NET 8 SDK** installed on the machine.
- A supported database target if you plan to switch from in-memory mode to SQL Server.
- The repository already includes Identity, EF Core, and MVC packages required to build and run the app.

### Clean, Build, and Run

From the project root (`c:\MPOnline`), run the following commands:

```powershell
dotnet clean
dotnet build
dotnet run --roll-forward Major
```

What each step does:

- `dotnet clean` removes old build output.
- `dotnet build` compiles the app and validates package restoration.
- `dotnet run --roll-forward Major` starts the application while allowing a newer installed major runtime to be used when necessary.

### Database Mode

The app starts with `UseInMemoryDatabase = true` in `appsettings.json`.

If you want SQL Server persistence instead:

1. Change `UseInMemoryDatabase` to `false`.
2. Confirm that `DefaultConnection` points to the correct SQL Server instance.
3. Run the application again so the seeder creates the initial records in the SQL-backed database.

### Default Seeded Credentials

The seeded administrator account is:

- **Admin**: `admin@mib.com`
- **Password**: `Admin@123`

The admin account is created automatically by the database seeder if it does not already exist.

## Database Seeding Summary

On startup, the application calls `DbSeeder.SeedAsync(...)` after building the service container. The seeder uses `EnsureCreatedAsync()` and then populates roles, the admin identity, and all M.I.B. domain data.

### Seeded Roles

- `Admin`
- `Librarian`
- `Student`

### Seeded Records

The current seed set includes:

- **80 Books**
- **50 Agents** (`StudentModel` records)
- **5 Staff Members** (`LibrarianModel` records)
- **14 Publications**
  - 4 newspapers
  - 10 magazines
- **15 Borrow Records**

### Seed Characteristics

Books are seeded with:

- M.I.B.-style classified titles,
- fake internal publisher names,
- randomized copy counts,
- staggered publication dates,
- unique ISBN-like identifiers.

Student records are seeded as agents with:

- `StudentCardId` values in the `MIB-###` format,
- `Agent` as the first name,
- rotating last-name markers,
- M.I.B. email addresses,
- active status enabled.

Librarians are seeded as staff with:

- structured employee IDs,
- canonical M.I.B. character-inspired names,
- active status enabled.

Borrow records are seeded using books and students already inserted into the context, with a mix of returned and active transactions. Due dates are generally 14 days after the borrow date, and a subset of records are intentionally overdue or still open to exercise dashboard telemetry.

## Troubleshooting & Maintenance

### Runtime `NullReferenceException`

The dashboard controller already guards against missing or failing borrow record queries by checking `_context.BorrowRecords != null` and wrapping the live metrics calculation in a `try/catch` block.

If you still see a null-reference at runtime, verify these points:

- `BorrowRecords` is declared in `ApplicationDbContext` as `DbSet<BorrowRecord>`.
- The database was created successfully before the dashboard query runs.
- The borrow record entities are being loaded with the expected `Book` and `Student` navigation properties.

### EF Core Context Mapping for `BorrowRecords`

The expected mapping chain is:

- `ApplicationDbContext` exposes `DbSet<BorrowRecord> BorrowRecords`.
- `Book` exposes a navigation collection of `BorrowRecords`.
- `BorrowRecord` contains `BookId`, `StudentId`, and nullable navigation properties.

If borrow-related pages fail, confirm the following:

- the model class name matches the `DbSet` property name,
- the seeder has run successfully,
- the in-memory mode was not reset between requests in a way that erases state unexpectedly,
- SQL Server migrations or schema creation were completed if the app is not running in-memory.

### Razor View Pagination Problems

The directory pages depend on controller-supplied paging values. Common issues usually come from missing or inconsistent view data.

If page navigation looks broken:

- make sure the controller populates `PageNumber`, `TotalItems`, and `TotalPages`.
- preserve the active search parameter when generating next/previous links.
- ensure page numbers are clamped to valid bounds before the query executes.
- keep the page-size values aligned between controller and view model.

Specific behavior already present in the repository:

- Books and librarians use strongly typed view models with paging helpers.
- Students and publications use `ViewBag`-based page metadata in the views.
- The views disable previous or next buttons when the page limit is reached.

### Borrow Workflow Stability

If borrowing fails or the stock count becomes inconsistent, review these rules:

- A borrow operation should only proceed when `AvailableCopies > 0`.
- The POST action removes `Book` and `Student` model-state validation entries before saving, because those navigation properties are populated by EF rather than form input.
- A return operation should only increment available copies once per record.

### Identity and Access Issues

If login works but privileged actions are blocked:

- confirm the user is assigned to the expected role,
- confirm the cookie login path still points to `/Account/Login`,
- verify the authorization attribute on the target controller action,
- ensure the seeded admin account still exists in the current database.

### Recommended Maintenance Routine

- Re-run `dotnet clean` and `dotnet build` after package updates.
- Validate the app with the in-memory database first, then test with SQL Server if production deployment will use it.
- Keep the seeder data deterministic enough for demos, but avoid depending on the exact random copy counts in automated tests.
- If you add new list pages, follow the existing paging pattern so the UI remains consistent.

## Operational Notes

- The application is started from `Program.cs`, where the database provider is chosen based on `UseInMemoryDatabase`.
- Identity roles and the default admin user are created during startup seeding.
- The UI uses a M.I.B. mission-control style presentation with icon-based cards and dark styling.
- The dashboard is intended to be the primary operational summary for librarians and administrators.

## Quick Reference

- **Project root**: `c:\MPOnline`
- **Main startup file**: `Program.cs`
- **Database context**: `Data/ApplicationDbContext.cs`
- **Seeder**: `Data/DbSeeder.cs`
- **Main dashboard**: `Controllers/DashboardController.cs`
- **Authentication**: `Controllers/AccountController.cs`
- **Default admin**: `admin@mib.com` / `Admin@123`

This documentation reflects the current repository state and should be updated whenever model fields, seed counts, controller access rules, or runtime configuration change.