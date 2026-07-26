using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Ensure database exists
            await context.Database.EnsureCreatedAsync();

            // -------------------- Roles --------------------
            string[] roles = { "Admin", "Librarian", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // -------------------- Users --------------------
            var adminEmail = "admin@mib.com";
            var admin = await userManager.FindByEmailAsync(adminEmail);
            if (admin == null)
            {
                admin = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                var result = await userManager.CreateAsync(admin, "Admin@123");
                if (result.Succeeded) await userManager.AddToRoleAsync(admin, "Admin");
            }

            // -------------------- Books --------------------
            if (!await context.Books.AnyAsync())
            {
                var books = new List<Book>();
                string[] authors = { "Dr. Laurel Weaver", "Tech Agent O", "Agent K", "Rosenberg", "HQ Armory", "Agent J", "Chief Zed", "Archival Core", "Dr. Mendel", "Linguistics Division" };
                string[] topics = { "Neuralyzer Mechanics", "Alien Anatomy", "Sub-Level Physics", "Wormhole Navigation", "Plasma Weaponry", "Diplomatic Protocols", "Universal Translation", "Disguise Technology", "Exoplanet Botany", "Deep Space Telemetry" };
                var rnd = new Random();
                for (int i = 1; i <= 80; i++)
                {
                    var author = authors[i % authors.Length];
                    var topic = topics[i % topics.Length];
                    books.Add(new Book
                    {
                        Title = $"{topic}: Classified Field Manual Vol. {i}",
                        Author = author,
                        ISBN = $"MIB-90{i:D2}-{100 + i}",
                        Publisher = (i % 2 == 0) ? "MIB R&D Division" : "Sub-Level 4 Press",
                        PublishDate = new DateTime(2010 + (i % 15), (i % 12) + 1, (i % 28) + 1),
                        TotalCopies = rnd.Next(5, 31),
                        AvailableCopies = rnd.Next(1, 6)
                    });
                }
                await context.Books.AddRangeAsync(books);
                await context.SaveChangesAsync(); // Persist books before dependent seeding
            }

            // -------------------- Students (Agents) --------------------
            if (!await context.Students.AnyAsync())
            {
                var students = new List<StudentModel>();
                char[] letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
                var rnd = new Random();
                for (int i = 1; i <= 50; i++)
                {
                    var letter = letters[(i - 1) % letters.Length];
                    students.Add(new StudentModel
                    {
                        StudentCardId = $"MIB-{i:D3}",
                        FirstName = "Agent",
                        LastName = letter.ToString(),
                        Email = $"agent{letter.ToString().ToLower()}{i}@mib.com",
                        EnrollmentDate = new DateTime(2020 + (i % 6), (i % 12) + 1, (i % 28) + 1),
                        IsActive = true
                    });
                }
                await context.Students.AddRangeAsync(students);
                await context.SaveChangesAsync(); // Persist students before dependent seeding
            }

            // -------------------- Librarians --------------------
            if (!await context.Librarians.AnyAsync())
            {
                var librarians = new List<LibrarianModel>
                {
                    new LibrarianModel { EmployeeId = "EMP-001", FirstName = "Chief", LastName = "Zed", Email = "zed@mib.com", HireDate = new DateTime(1997, 6, 1), IsActive = true },
                    new LibrarianModel { EmployeeId = "EMP-002", FirstName = "Agent", LastName = "O", Email = "agento@mib.com", HireDate = new DateTime(2012, 5, 25), IsActive = true },
                    new LibrarianModel { EmployeeId = "EMP-003", FirstName = "Frank", LastName = "The Pug", Email = "frank@mib.com", HireDate = new DateTime(2002, 7, 3), IsActive = true },
                    new LibrarianModel { EmployeeId = "EMP-004", FirstName = "Agent", LastName = "K", Email = "agentk@mib.com", HireDate = new DateTime(1997, 7, 2), IsActive = true },
                    new LibrarianModel { EmployeeId = "EMP-005", FirstName = "Dr. Laurel", LastName = "Weaver", Email = "lweaver@mib.com", HireDate = new DateTime(1997, 8, 15), IsActive = true }
                };
                await context.Librarians.AddRangeAsync(librarians);
            }

            // -------------------- Publications --------------------
            if (!await context.Publications.AnyAsync())
            {
                var publications = new List<Publication>();
                // 4 Newspapers
                publications.Add(new Publication { Title = "The Daily Inquirer (Alien Sightings Ed.)", Type = PublicationType.Newspaper, IssueNumber = "Vol 44 No. 12", ReleaseDate = new DateTime(2026, 7, 1), Publisher = "Tabloid Press HQ" });
                publications.Add(new Publication { Title = "New York World News - Classified Ed.", Type = PublicationType.Newspaper, IssueNumber = "Vol 102 No. 8", ReleaseDate = new DateTime(2026, 5, 30), Publisher = "Global Archival" });
                publications.Add(new Publication { Title = "The Weekly Midnight Globe", Type = PublicationType.Newspaper, IssueNumber = "Vol 19 No. 44", ReleaseDate = new DateTime(2026, 6, 12), Publisher = "Suburban Press" });
                publications.Add(new Publication { Title = "Post-Cosmic Gazette", Type = PublicationType.Newspaper, IssueNumber = "Vol 88 No. 01", ReleaseDate = new DateTime(2026, 7, 10), Publisher = "Sector 4 Communications" });
                // 10 Magazines
                for (int i = 1; i <= 10; i++)
                {
                    publications.Add(new Publication
                    {
                        Title = $"Intergalactic Monthly Digest #{i}",
                        Type = PublicationType.Magazine,
                        IssueNumber = $"Issue {800 + i}",
                        ReleaseDate = new DateTime(2026, (i % 6) + 1, (i % 20) + 1),
                        Publisher = (i % 2 == 0) ? "Sector 4 Periodicals" : "Breakroom Publishing"
                    });
                }
                await context.Publications.AddRangeAsync(publications);
            }

            // -------------------- Borrow Records --------------------
            if (!await context.BorrowRecords.AnyAsync())
            {
                var books = await context.Books.Take(30).ToListAsync();
                var students = await context.Students.Take(30).ToListAsync();
                var rnd = new Random();
                var records = new List<BorrowRecord>();
                for (int i = 0; i < 15; i++)
                {
                    var book = books[rnd.Next(books.Count)];
                    var student = students[rnd.Next(students.Count)];
                    var borrowDate = DateTime.Today.AddDays(-(i * 3 + rnd.Next(1, 4)));
                    var dueDate = borrowDate.AddDays(14);
                    var isReturned = i % 3 == 0;
                    records.Add(new BorrowRecord
                    {
                        BookId = book.Id,
                        StudentId = student.Id,
                        BorrowDate = borrowDate,
                        DueDate = dueDate,
                        IsReturned = isReturned,
                        ReturnDate = isReturned ? (DateTime?)borrowDate.AddDays(rnd.Next(1, 10)) : null
                    });
                }
                await context.BorrowRecords.AddRangeAsync(records);
            }

            // Persist all changes
            await context.SaveChangesAsync();
        }
    }
}