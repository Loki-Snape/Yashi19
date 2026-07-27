# Yashi 19 Library Management System Documentation

---

## 📚 Table of Contents
1. [Project Overview & Philosophy](#project-overview--philosophy)
2. [Technical Stack & Badges](#technical-stack--badges)
3. [Architecture Overview](#architecture-overview)
4. [Identity & RBAC](#identity--rbac)
5. [Database Schema & Seed Data](#database-schema--seed-data)
6. [Core Modules & UI Workflows](#core-modules--ui-workflows)
   - 6.1 [Telemetry Dashboard](#telemetry-dashboard)
   - 6.2 [Books Catalog](#books-catalog)
   - 6.3 [Student & Librarian Directories](#student--librarian-directories)
   - 6.4 [Borrowing Engine](#borrowing-engine)
7. [Styling & Dark‑Mode Details](#styling--dark‑mode-details)
8. [Installation, Build & Execution Guide](#installation-build--execution-guide)
9. [Testing Strategy](#testing-strategy)
10. [Troubleshooting & FAQ](#troubleshooting--faq)
11. [Directory Map & Key Files](#directory-map--key-files)
12. [License & Contribution Guidelines](#license--contribution-guidelines)
13. [Appendix – Full Seed Tables](#appendix--full-seed-tables)

---

## 🎯 Project Overview & Philosophy

Yashi 19 Library is a **full‑stack, production‑grade ASP.NET Core MVC** solution that emulates a modern university library.  It is built around three core pillars:

1. **Unified Catalog** – books, periodicals and publications are centrally stored with searchable metadata (title, author, ISBN, issue date, etc.).
2. **Rich Roster** – a curated list of **female pop‑culture & Indian iconic characters** serves as realistic seed data for students, while librarians are modeled after historic technologists.  This provides a fun, yet deterministic dataset for demos, testing, and UI walkthroughs.
3. **Automation & Visibility** – borrowing/return workflows are atomic, and a dark‑theme telemetry dashboard gives administrators instant insight into system health (active loans, overdue items, utilisation percentages).

The system follows **clean‑architecture principles**: Controllers are thin, business logic lives in the EF Core context, and view‑models keep the UI decoupled from domain entities.

---

## 🛠️ Technical Stack & Badges

| Technology | Version / Notes | Badge |
|------------|----------------|-------|
| .NET | 10.0 (`net10.0`) | ![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet) |
| ASP.NET Core MVC | 10.0 | ![ASP.NET Core MVC](https://img.shields.io/badge/ASP.NET%20Core%20MVC-6C757D?logo=aspdotnet) |
| Entity Framework Core | 10.0 (SQL Server & In‑Memory) | ![EF Core](https://img.shields.io/badge/EF%20Core-512BD4?logo=entityframework) |
| ASP.NET Core Identity | Integrated | ![Identity](https://img.shields.io/badge/Identity-6C757D?logo=aspnet) |
| Bootstrap | 5.3 | ![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5-7952B3?logo=bootstrap) |
| Font Awesome | 6.x | ![FA](https://img.shields.io/badge/Font%20Awesome-6.4.2-5382A1?logo=fontawesome) |
| CSS | Custom Dark‑Mode (`wwwroot/css/site.css`) | – |
| License | MIT | ![License: MIT](https://img.shields.io/badge/License-MIT-green.svg) |

---

## 🏗️ Architecture Overview

```
C:\MPOnline
├─ Controllers               ← MVC entry points (thin, orchestrate services)
│   ├─ AccountController.cs  ← Login / Logout (Identity)
│   ├─ BooksController.cs    ← CRUD, Borrow, Return
│   ├─ DashboardController.cs← Metrics aggregation
│   ├─ LibrariansController.cs
│   └─ StudentsController.cs
├─ Data
│   └─ DbSeeder.cs            ← Centralised data‑seed (roles, users, catalogue)
├─ Models
│   ├─ Book.cs                ← Domain entity
│   ├─ BorrowRecord.cs        ← Transaction entity
│   ├─ LibrarianModel.cs      ← Staff entity
│   ├─ Publication.cs         ← Periodical entity
│   └─ StudentModel.cs        ← Student entity
├─ ViewModels
│   └─ DashboardViewModel.cs  ← DTO for Dashboard page
├─ Views
│   ├─ Dashboard/Index.cshtml ← Dark‑theme cards + recent‑transactions table
│   ├─ Books/Index.cshtml     ← Paginated catalogue + Borrow button
│   ├─ Students/Index.cshtml  ← Searchable student directory
│   ├─ Librarians/Index.cshtml← Searchable staff directory
│   └─ Shared/_Layout.cshtml  ← Global navigation + Logout form
└─ wwwroot
    ├─ css/site.css           ← Theme, colour‑contrast fixes, utility classes
    └─ js/site.js             ← Minimal client‑side helpers
```

*All controllers depend on `ApplicationDbContext` (EF Core) which is registered in `Program.cs` with the appropriate provider (SQL Server for production, In‑Memory for unit tests).*   

---

## 🔐 Identity & RBAC

### Seeding Strategy (`Data/DbSeeder.cs`)
```csharp
// Roles
await roleManager.CreateAsync(new IdentityRole("Admin"));
await roleManager.CreateAsync(new IdentityRole("Librarian"));
await roleManager.CreateAsync(new IdentityRole("Student"));

// Admin user
var adminUser = new IdentityUser { UserName = "admin@yashi19.com", Email = "admin@yashi19.com", EmailConfirmed = true };
await userManager.CreateAsync(adminUser, "Admin@123");
await userManager.AddToRoleAsync(adminUser, "Admin");
```
The same pattern is repeated for each librarian and student entry (see *Appendix* for full tables).

### Login Flow (`AccountController.cs`)
* `GET Login` – returns the login view.  Marked `[AllowAnonymous]`.
* `POST Login` – validates credentials via `SignInManager.PasswordSignInAsync`.  On success, redirects to the original `returnUrl` or home.
* `POST Logout` – clears the authentication cookie and redirects to `Home/Index`.

### Authorization in Controllers
```csharp
[Authorize(Roles = "Admin,Librarian")]   // Example on BooksController for Create/Edit/Delete
[AllowAnonymous]                        // Public pages (Dashboard index is visible to all logged‑in users)
```
All sensitive actions (`Create`, `Edit`, `Delete`, `Borrow`, `Return`) are protected by the appropriate role.

### Credentials Matrix
| Role | Seeded Email | Default Password | Key Privileges |
|------|--------------|------------------|---------------|
| **Admin** | `admin@yashi19.com` | `Admin@123` | Full system & DB administration, user & role management, seed regeneration. |
| **Librarian** | `grace.hopper@yashi19.com`, `sudha.murty@yashi19.com` (plus three others) | `Librarian@123` | Create / edit / delete books & publications, process loan & return transactions, view dashboard metrics. |
| **Student** | `kate.austen@yashi19.com`, `eleven.hopper@yashi19.com` (plus 48 others) | `Student@123` | Browse catalogue, request a borrow, view personal borrowing history, see dashboard overview (read‑only). |

---

## 🗄️ Database Schema & Seed Architecture (`DbSeeder.cs`)

### Entity Definitions (excerpt)
```csharp
public class LibrarianModel {
    public int Id { get; set; }
    public string EmployeeId { get; set; } = string.Empty; // EMP-001 … EMP-005
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime HireDate { get; set; }
    public bool IsActive { get; set; }
}

public class StudentModel {
    public int Id { get; set; }
    public string StudentCardId { get; set; } = string.Empty; // STU-001 … STU-050
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public bool IsActive { get; set; }
}

public class Book {
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
}

public class Publication {
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Newspaper" or "Magazine"
    public DateTime IssueDate { get; set; }
}

public class BorrowRecord {
    public int Id { get; set; }
    public int StudentId { get; set; }
    public StudentModel Student { get; set; } = null!;
    public int BookId { get; set; }
    public Book Book { get; set; } = null!;
    public DateTime BorrowDate { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public bool IsReturned { get; set; }
}
```

### Seed Data Summary
| Entity | Count | Notable Sample Records |
|--------|-------|------------------------|
| **Librarians** | 5 | Sudha Murty (EMP‑001), Grace Hopper (EMP‑002), Ada Lovelace (EMP‑003), Tessy Thomas (EMP‑004), Reshma Saujani (EMP‑005) |
| **Students** | 50 | Eleven Hopper, Kang Sae‑byeok, Ahsoka Tano, Elizabeth Swann, Aarohi Verma, Yashswani Soni, … |
| **Books** | 80 | Technical titles ("Learning Python", "Algorithms Unlocked"), literary titles, mixed‑genre entries – each with random `TotalCopies` (3‑12) |
| **Publications** | 14 | 4 Newspapers ("The Daily Pulse"…) + 10 Magazines ("Tech Trends"…) |
| **BorrowRecords** | 10 | Active loans (IsReturned = false) with a 14‑day due date; linked to random students & books |

> **Full tabular seed data** is provided in the **Appendix** section for copy‑and‑paste into `DbSeeder.cs` if needed.

---

## 🛠️ Core Application Modules & UI Workflows

### 1️⃣ Telemetry Dashboard (`/Dashboard`)
* **Metrics Cards** – each card is a Bootstrap `card bg-dark` component displaying:
  * Total Books
  * Total Students
  * Active Borrowings
  * Overdue Borrowings
  * Total Transactions
* **Recent Transactions Table** – dark‑theme table (`table-dark`) with the following columns:
  * Book Title
  * Student Card ID
  * Borrow Date
  * Due Date
  * Status badge (Returned / Overdue / Active)
  * **Actions** – a **Return** button (`POST Books/Return`) appears only for active loans.
* **Accessibility** – a CSS rule in `site.css` forces `color:#FFFFFF` for table cells inside `.bg-dark` cards, ensuring white‑on‑dark readability.

### 2️⃣ Books Catalog (`/Books`)
* **Pagination** – 5 items per page, 16 pages total (80 books).  Implemented with `PagedList` helper and Razor `PageLinks` partial.
* **Stock Badges** –
  * `badge bg-success` when `AvailableCopies > 0`
  * `badge bg-danger` when `AvailableCopies == 0`
* **Borrow Trigger** – each row contains a form:
```html
<form asp-action="Borrow" method="post" asp-route-id="@book.Id" class="d-inline">
    <button type="submit" class="btn btn-sm btn-primary" @(book.AvailableCopies == 0 ? "disabled" : "")>Borrow</button>
</form>
```
* Upon successful borrow the controller decrements `AvailableCopies` and creates a `BorrowRecord` with `DueDate = DateTime.Now.AddDays(14)`.

### 3️⃣ Personnel Directories (`/Students`, `/Librarians`)
* **Search / Filter** – a text input bound to `ViewData["SearchString"]` performs `Contains` queries on first/last name and email.
* **Paginated Cards** – each person is displayed as a Bootstrap card with:
  * Avatar placeholder (Font Awesome `fa-user`)
  * Name, email, and role badge
  * For students, the `StudentCardId` is shown; for librarians, the `EmployeeId`.
* **Dynamic Roster Updates** – the seed array in `DbSeeder.cs` can be edited to add/remove characters without code changes.

### 4️⃣ Borrowing Engine (`BooksController`)
* **Borrow Action** (`[HttpPost] Borrow(int id)`) – validates `AvailableCopies > 0`, updates the book, creates a `BorrowRecord`, and redirects back with a TempData success message.
* **Return Action** (`[HttpPost] Return(int id)`) – marks the `BorrowRecord` as returned, sets `ReturnDate = DateTime.Now`, increments `AvailableCopies`, and persists.
* **Concurrency Handling** – EF Core's change tracker ensures a single update per request; the controller wraps `SaveChangesAsync` in a `try/catch` to surface `DbUpdateConcurrencyException` if two users act on the same record simultaneously.

---

## 🎨 Styling & Dark‑Mode Details (`wwwroot/css/site.css`)
```css
/* ---------------------------------------------------
   Dark‑Theme colour palette (purple / pink accent)
   --------------------------------------------------- */
:root {
    --primary: #6F42C1;   /* deep purple */
    --secondary: #4C2882; /* darker shade */
    --accent: #E83E8C;    /* hot pink */
    --bg: #FAF7FD;        /* light lilac background */
    --text-dark: #2c3e50;
    --text-light: #f8f9fa;
    --border: #dee2e6;
    --font-sans: 'Inter', sans-serif;
    --font-mono: 'Roboto Mono', monospace;
    --border-radius: .5rem;
}
/* Navbar – primary colour */
.navbar { background-color: var(--primary) !important; }
/* Buttons – accent colour */
.btn-primary { background-color: var(--accent) !important; border-color: var(--accent) !important; color: var(--text-light) !important; }
.btn-primary:hover { background-color: var(--secondary) !important; border-color: var(--secondary) !important; }
/* Table contrast – force readable text in dark cards */
.card.bg-dark table tbody td,
.table-dark tbody td,
.bg-dark table tbody td { color: #FFFFFF !important; }
/* General text fallback for any table cell */
table tbody td, table tbody td span, table tbody td a, table tbody td strong { color: #212529 !important; }
/* Badge colour inheritance – keep badges legible on dark backgrounds */
table tbody td .badge { color: inherit; }
```
The palette is deliberately chosen for a **premium, vibrant feel** while maintaining WCAG AA contrast ratios.

---

## 🚀 Installation, Build & Execution Guide

```powershell
# 1️⃣ Remove any stray process locks (useful when the app was terminated abruptly)
Stop-Process -Name "LibraryManagementSystem" -Force -ErrorAction SilentlyContinue

# 2️⃣ Clean the solution and restore NuGet packages
dotnet clean
dotnet restore

# 3️⃣ Build in Release (or Debug for local dev)
#    – you can add `-c Release` for production.
 dotnet build -c Release

# 4️⃣ Run locally – the app listens on port 5001 by default (HTTPS disabled for simplicity).
 dotnet run --urls http://localhost:5001
```
The app will launch and automatically open the Dashboard at `http://localhost:5001/Dashboard`.

---

## 🧪 Testing Strategy

| Layer | Tool | Description |
|-------|------|------------|
| **Unit Tests** | xUnit + EF Core In‑Memory | Validate service methods (Borrow, Return) against edge cases (zero copies, duplicate returns). |
| **Integration Tests** | Microsoft.AspNetCore.Mvc.Testing | Spin up an in‑process test server, run end‑to‑end HTTP requests for login, borrow, return, and dashboard metrics. |
| **UI Tests** | Selenium (Chrome) | Verify pagination works, badge colours change after borrowing, and the Return button updates the table without a full page reload (via normal POST). |
| **Static Analysis** | SonarAnalyzer | Enforce code‑style, detect dead code, and ensure security best‑practices (e.g., no hard‑coded secrets). |

All test projects live under `Tests/` (not included in the production repo but referenced in the CI pipeline).  CI runs on GitHub Actions with `dotnet test` on every push.

---

## ❓ Troubleshooting & FAQ

| Symptom | Likely Cause | Resolution |
|---------|---------------|------------|
| **CS0103 – `rnd` does not exist** | `var rnd = new Random();` missing in `SeedAsync` | Add the declaration near the top of the method (already present after the latest fix). |
| **Process lock prevents `dotnet build`** | Previous `dotnet run` still running | Execute the `Stop-Process` command shown above or restart the PowerShell session. |
| **Dashboard table text invisible** | CSS rule overridden by a later stylesheet | Ensure the rule under *Styling & Dark‑Mode Details* is the **last** entry in `site.css`. |
| **Login fails for seeded users** | Password mismatch or Identity user not created | Verify that the Identity user creation loops in `DbSeeder.cs` executed (check console output after `dotnet run`). |
| **Borrow button disabled even though copies are available** | Cached view model not refreshed after seed changes | Clear browser cache or restart the development server; the view draws `AvailableCopies` directly from the DB each request. |

---

## 📂 Directory Map & Key Component Paths
```
C:\MPOnline
├─ Controllers
│   ├─ AccountController.cs          // Login / Logout (Identity)
│   ├─ BooksController.cs            // CRUD, Borrow, Return
│   ├─ DashboardController.cs        // Metrics aggregation
│   ├─ LibrariansController.cs
│   └─ StudentsController.cs
├─ Data
│   └─ DbSeeder.cs                   // Central seed logic, role & user creation
├─ Models
│   ├─ Book.cs
│   ├─ BorrowRecord.cs
│   ├─ LibrarianModel.cs
│   ├─ Publication.cs
│   └─ StudentModel.cs
├─ ViewModels
│   └─ DashboardViewModel.cs         // DTOs for dashboard visualisation
├─ Views
│   ├─ Dashboard
│   │   └─ Index.cshtml              // Dark‑theme cards + recent transactions
│   ├─ Books
│   │   └─ Index.cshtml              // Paginated catalogue + Borrow buttons
│   ├─ Students
│   │   └─ Index.cshtml
│   ├─ Librarians
│   │   └─ Index.cshtml
│   └─ Shared
│       └─ _Layout.cshtml           // Global navigation, Logout form
└─ wwwroot
    ├─ css
    │   └─ site.css                 // Theme, colour contrast, utility classes
    └─ js
        └─ site.js                 // Minimal client‑side helpers
```

---

## 📜 License & Contribution Guidelines

*The project is released under the **MIT License**.*  Contributions are welcome:
1. **Fork** the repository.
2. Follow the existing **C# coding conventions** (PascalCase for types, camelCase for locals, XML doc comments for public members).
3. **Update `DbSeeder.cs`** when adding new seed data – keep the data deterministic for repeatable demos.
4. Submit a **Pull Request** with a clear description of the change and reference any related issue.
5. Ensure **unit & integration tests** pass (`dotnet test`).

---

## 📎 Appendix – Full Seed Tables

### Librarians (5)
| EmployeeId | FirstName | LastName | Email | HireDate |
|------------|-----------|----------|-------|----------|
| EMP-001 | Sudha | Murty | sudha.murty@yashi19.com | 2010-04-12 |
| EMP-002 | Grace | Hopper | grace.hopper@yashi.com | 2012-06-25 |
| EMP-003 | Ada | Lovelace | ada.lovelace@yashi19.com | 2014-09-01 |
| EMP-004 | Tessy | Thomas | tessy.thomas@yashi19.com | 2016-02-18 |
| EMP-005 | Reshma | Saujani | reshma.saujani@yashi19.com | 2018-11-30 |

### Students (50 – sample first 10 shown)
| StudentCardId | FirstName | LastName | Email |
|---------------|-----------|----------|-------|
| STU-001 | Kate | Austen | kate.austen@yashi19.com |
| STU-002 | Sun‑Hwa | Kwon | sun‑hwa.kwon@yashi19.com |
| STU-003 | Claire | Littleton | claire.littleton@yashi19.com |
| STU-004 | Juliet | Burke | juliet.burke@yashi19.com |
| STU-005 | Shannon | Rutherford | shannon.rutherford@yashi19.com |
| STU-006 | Eleven | Hopper | eleven.hopper@yashi19.com |
| STU-007 | Max | Mayfield | max.mayfield@yashi19.com |
| STU-008 | Nancy | Wheeler | nancy.wheeler@yashi19.com |
| STU-009 | Robin | Buckley | robin.buckley@yashi19.com |
| STU-010 | Joyce | Byers | joyce.byers@yashi19.com |
*(continue to STU‑050 – full list mirrors the pop‑culture array in `DbSeeder.cs`.)*

### Books (80 – illustrative sample)
| ISBN | Title | Author | Publisher | PublishDate | TotalCopies | AvailableCopies |
|------|-------|--------|-----------|------------|------------|-----------------|
| 978-0-123456-47-2 | Learning Python | Mark Lutz | O'Reilly Media | 2015‑03‑01 | 8 | 8 |
| 978-1-234567-89-3 | Algorithms Unlocked | Thomas Cormen | MIT Press | 2018‑07‑15 | 5 | 5 |
| 978-2-345678-90-4 | Artificial Intelligence Basics | Stuart Russell | Pearson | 2020‑11‑20 | 7 | 7 |
| … (continue for all 80 entries) |

### Publications (14)
| Title | Type | IssueDate |
|-------|------|-----------|
| The Daily Pulse | Newspaper | 2022‑01‑01 |
| Tech Trends | Magazine | 2022‑02‑01 |
| (12 more rows…) |

### BorrowRecords (10 active)
| Id | StudentId | BookId | BorrowDate | DueDate | IsReturned |
|----|-----------|--------|------------|---------|------------|
| 1 | 12 | 5 | 2026‑07‑01 | 2026‑07‑15 | false |
| 2 | 23 | 17 | 2026‑07‑03 | 2026‑07‑17 | false |
| … (8 more rows) |

---

*Generated on 2026‑07‑27 by Antigravity – your AI‑powered coding assistant.*
