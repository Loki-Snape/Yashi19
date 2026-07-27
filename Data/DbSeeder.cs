using System;
using System.Collections.Generic;
using System.Linq;
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
            var rnd = new Random();
            // Ensure DB exists
            await context.Database.EnsureCreatedAsync();

            // ---------- Roles ----------
            string[] roles = { "Admin", "Librarian", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ---------- Admin User ----------
            var adminEmail = "admin@yashi19.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
            else
            {
                // Ensure password is reset to Admin@123 if user exists
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                await userManager.ResetPasswordAsync(adminUser, token, "Admin@123");
            }

            // ---------- Clear Existing Data ----------
            context.BorrowRecords.RemoveRange(context.BorrowRecords);
            context.Books.RemoveRange(context.Books);
            context.Publications.RemoveRange(context.Publications);
            context.Students.RemoveRange(context.Students);
            context.Librarians.RemoveRange(context.Librarians);
            await context.SaveChangesAsync();

            // ---------- Librarians (5 women) ----------
            var librarians = new List<LibrarianModel>
            {
                new LibrarianModel { EmployeeId = "EMP-001", FirstName = "Sudha", LastName = "Murty", Email = "sudha.murty@yashi19.com", HireDate = new DateTime(2010,4,12), IsActive = true },
                new LibrarianModel { EmployeeId = "EMP-002", FirstName = "Grace", LastName = "Hopper", Email = "grace.hopper@yashi19.com", HireDate = new DateTime(2012,6,25), IsActive = true },
                new LibrarianModel { EmployeeId = "EMP-003", FirstName = "Ada", LastName = "Lovelace", Email = "ada.lovelace@yashi19.com", HireDate = new DateTime(2014,9,1), IsActive = true },
                new LibrarianModel { EmployeeId = "EMP-004", FirstName = "Tessy", LastName = "Thomas", Email = "tessy.thomas@yashi19.com", HireDate = new DateTime(2016,2,18), IsActive = true },
                new LibrarianModel { EmployeeId = "EMP-005", FirstName = "Reshma", LastName = "Saujani", Email = "reshma.saujani@yashi19.com", HireDate = new DateTime(2018,11,30), IsActive = true }
            };
            await context.Librarians.AddRangeAsync(librarians);

            // Create Identity users for Librarians (Password: Librarian@123)
            foreach (var lib in librarians)
            {
                var libUser = await userManager.FindByEmailAsync(lib.Email);
                if (libUser == null)
                {
                    libUser = new IdentityUser { UserName = lib.Email, Email = lib.Email, EmailConfirmed = true };
                    var result = await userManager.CreateAsync(libUser, "Librarian@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(libUser, "Librarian");
                    }
                }
            }

            // ---------- Students (50 female pop‑culture characters) ----------
            var popCultureFemaleCharacters = new (string First, string Last)[]
            {
                // Lost
                ("Kate", "Austen"), ("Sun-Hwa", "Kwon"), ("Claire", "Littleton"), ("Juliet", "Burke"), ("Shannon", "Rutherford"),
                
                // Stranger Things
                ("Eleven", "Hopper"), ("Max", "Mayfield"), ("Nancy", "Wheeler"), ("Robin", "Buckley"), ("Joyce", "Byers"),
                
                // Star Wars
                ("Princess", "Leia"), ("Rey", "Skywalker"), ("Ahsoka", "Tano"), ("Padmé", "Amidala"), ("Jyn", "Erso"), ("Hera", "Syndulla"),
                
                // From
                ("Tabitha", "Matthews"), ("Donna", "Raines"), ("Kristi", "Miller"), ("Julie", "Matthews"), ("Fatima", "Hassan"),
                
                // Squid Game
                ("Kang", "Sae-byeok"), ("Ji-yeong", "Lee"), ("Han", "Mi-nyeo"), ("Cho", "Sang-woo-Mom"),
                
                // Pirates of the Caribbean
                ("Elizabeth", "Swann"), ("Carina", "Smyth"), ("Tia", "Dalma"), ("Anamaria", "Pirate"),
                
                // MCU
                ("Wanda", "Maximoff"), ("Natasha", "Romanoff"), ("Carol", "Danvers"), ("Shuri", "Udaku"), ("Peggy", "Carter"),
                ("Gamora", "Zen"), ("Nebula", "Titan"), ("Sylvie", "Laufeydottir"), ("Kate", "Bishop"), ("Yelena", "Belova"),
                ("Elizabeth", "Ross"), ("Monica", "Rambeau"), ("America", "Chavez"), ("Hope", "van Dyne"), ("Jane", "Foster"),
                ("Aarohi", "Verma"), ("Priya", "Sharma"), ("Ananya", "Deshmukh"), ("Diya", "Iyer"), ("Yashswani", "Soni"), ("Meera", "Patel")
            };

            var students = new List<StudentModel>();
            for (int i = 0; i < 50; i++)
            {
                var character = popCultureFemaleCharacters[i % popCultureFemaleCharacters.Length];
                var cleanFirst = character.First.ToLower().Replace(" ", "").Replace("-", "");
                var cleanLast = character.Last.ToLower().Replace(" ", "").Replace("-", "");

                students.Add(new StudentModel
                {
                    StudentCardId = $"STU-{(i + 1):D3}",
                    FirstName = character.First,
                    LastName = character.Last,
                    Email = $"{cleanFirst}.{cleanLast}@yashi19.com",
                    EnrollmentDate = DateTime.Now.AddDays(-((i * 17) % 800)),
                    IsActive = true
                });
            }
            await context.Students.AddRangeAsync(students);

            // Create Identity users for Students (Password: Student@123)
            foreach (var stu in students)
            {
                var stuUser = await userManager.FindByEmailAsync(stu.Email);
                if (stuUser == null)
                {
                    stuUser = new IdentityUser { UserName = stu.Email, Email = stu.Email, EmailConfirmed = true };
                    var result = await userManager.CreateAsync(stuUser, "Student@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(stuUser, "Student");
                    }
                }
            }

            // ---------- Books (80 items) ----------
            var bookTopics = new[] {
                "Learning Python", "Data Structures in C++", "Algorithms Unlocked", "Artificial Intelligence Basics",
                "Machine Learning for Everyone", "Introduction to Quantum Computing", "Cybersecurity Fundamentals",
                "Women in Tech: Stories", "Lean In", "The Code Breaker", "To Kill a Mockingbird", "Wings of Fire",
                "The Pragmatic Programmer", "Clean Code", "Design Patterns", "Effective Java", "Modern JavaScript", "HTML & CSS", "Docker Deep Dive",
                "Kubernetes in Action"
            };
            var books = new List<Book>();
            for (int i = 1; i <= 80; i++)
            {
                var topic = bookTopics[i % bookTopics.Length];
                books.Add(new Book
                {
                    Title = $"{topic} Volume {i}",
                    Author = $"Author {i}",
                    ISBN = $"978-0-{i:D5}-{rnd.Next(1000,9999)}",
                    Publisher = (i % 2 == 0) ? "Tech Press" : "Innovation Books",
                    PublishDate = DateTime.Today.AddDays(-rnd.Next(100, 2000)),
                    TotalCopies = rnd.Next(5, 20),
                    AvailableCopies = rnd.Next(1, 5)
                });
            }
            await context.Books.AddRangeAsync(books);

            // ---------- Publications (14 items) ----------
            var publications = new List<Publication>
            {
                new Publication { Title = "The Women Tech Gazette", Type = PublicationType.Newspaper, IssueNumber = "Vol 1 No. 1", ReleaseDate = new DateTime(2023,1,15), Publisher = "Tech Media" },
                new Publication { Title = "Women in STEM Monthly", Type = PublicationType.Magazine, IssueNumber = "Issue 10", ReleaseDate = new DateTime(2023,2,1), Publisher = "STEM Publishers" },
                new Publication { Title = "Innovators Quarterly", Type = PublicationType.Magazine, IssueNumber = "Q1 2023", ReleaseDate = new DateTime(2023,3,1), Publisher = "Innovate Ltd" },
                new Publication { Title = "Future Leaders Journal", Type = PublicationType.Magazine, IssueNumber = "2023-04", ReleaseDate = new DateTime(2023,4,10), Publisher = "Future Press" },
                new Publication { Title = "Tech Women Daily", Type = PublicationType.Newspaper, IssueNumber = "Vol 2 No. 5", ReleaseDate = new DateTime(2023,5,5), Publisher = "Daily Tech" },
                new Publication { Title = "Coding Her", Type = PublicationType.Magazine, IssueNumber = "Issue 22", ReleaseDate = new DateTime(2023,6,12), Publisher = "Code Publishers" },
                new Publication { Title = "Science Her", Type = PublicationType.Magazine, IssueNumber = "Issue 7", ReleaseDate = new DateTime(2023,7,20), Publisher = "Science House" },
                new Publication { Title = "The Quantum Quarterly", Type = PublicationType.Magazine, IssueNumber = "Q2 2023", ReleaseDate = new DateTime(2023,8,18), Publisher = "Quantum Press" },
                new Publication { Title = "Tech Insights", Type = PublicationType.Newspaper, IssueNumber = "Vol 3 No. 2", ReleaseDate = new DateTime(2023,9,2), Publisher = "Insights Media" },
                new Publication { Title = "Women Innovators Review", Type = PublicationType.Magazine, IssueNumber = "Rev 5", ReleaseDate = new DateTime(2023,10,11), Publisher = "Review Corp" },
                new Publication { Title = "Digital Dreams", Type = PublicationType.Magazine, IssueNumber = "Issue 13", ReleaseDate = new DateTime(2023,11,9), Publisher = "Dream Media" },
                new Publication { Title = "The Code Chronicle", Type = PublicationType.Newspaper, IssueNumber = "Vol 4 No. 8", ReleaseDate = new DateTime(2023,12,3), Publisher = "Chronicle Ltd" },
                new Publication { Title = "AI & Women", Type = PublicationType.Magazine, IssueNumber = "2023-12", ReleaseDate = new DateTime(2023,12,20), Publisher = "AI Press" },
                new Publication { Title = "Future Horizons", Type = PublicationType.Magazine, IssueNumber = "Issue 30", ReleaseDate = new DateTime(2024,1,15), Publisher = "Horizon Publishers" }
            };
            await context.Publications.AddRangeAsync(publications);

// ---------- Borrow Records (10 active) ----------
            // Ensure students and books have been saved so IDs exist
            await context.SaveChangesAsync();

            // Clear old borrow records
            context.BorrowRecords.RemoveRange(context.BorrowRecords);
            await context.SaveChangesAsync();

            var studentsList = await context.Students.ToListAsync();
            var booksList = await context.Books.ToListAsync();

            if (studentsList.Any() && booksList.Any())
            {
                var sampleBorrows = new List<BorrowRecord>();
                for (int i = 0; i < 10; i++)
                {
                    var borrowDate = DateTime.Now.AddDays(-rnd.Next(1, 10));
                    sampleBorrows.Add(new BorrowRecord
                    {
                        StudentId = studentsList[i % studentsList.Count].Id,
                        BookId = booksList[rnd.Next(booksList.Count)].Id,
                        BorrowDate = borrowDate,
                        ReturnDate = null,
                        IsReturned = false
                    });
                }

                context.BorrowRecords.AddRange(sampleBorrows);
                await context.SaveChangesAsync();
            }

            // Save everything
            await context.SaveChangesAsync();
        }
    }
}