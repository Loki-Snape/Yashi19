using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LibraryManagementSystem.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalBooks = await _context.Books.CountAsync(),
                TotalPublications = await _context.Publications.CountAsync(),
                TotalStudents = await _context.Students.CountAsync(),
                TotalLibrarians = await _context.Librarians.CountAsync(),
                ActiveBorrowings = 0,
                OverdueBorrowings = 0,
                TotalTransactions = 0,
                TotalBorrowings = 0,
                RecentTransactions = new List<RecentTransactionViewModel>(),
                RecentBorrowings = new List<RecentTransactionViewModel>()
            };

            try
            {
                if (_context.BorrowRecords != null)
                {
                    viewModel.ActiveBorrowings = await _context.BorrowRecords.CountAsync(b => !b.IsReturned);
                    viewModel.OverdueBorrowings = await _context.BorrowRecords.CountAsync(b => !b.IsReturned && b.DueDate < DateTime.Now);
                    viewModel.TotalTransactions = await _context.BorrowRecords.CountAsync();
                    viewModel.TotalBorrowings = viewModel.TotalTransactions;
                    viewModel.RecentTransactions = await _context.BorrowRecords
                        .Include(b => b.Book)
                        .Include(b => b.Student)
                        .OrderByDescending(b => b.BorrowDate)
                        .Take(5)
                        .Select(b => new RecentTransactionViewModel
                        {
                            BookTitle = b.Book != null ? b.Book.Title : "Unknown Asset",
                            StudentName = b.Student != null ? $"{b.Student.FirstName} {b.Student.LastName}" : "Unknown Agent",
                            StudentCardId = b.Student != null ? b.Student.StudentCardId : "N/A",
                            BorrowDate = b.BorrowDate,
                            DueDate = b.DueDate,
                            IsReturned = b.IsReturned
                        })
                        .ToListAsync();

                    viewModel.RecentBorrowings = viewModel.RecentTransactions;
                }
            }
            catch
            {
                viewModel.RecentTransactions = new List<RecentTransactionViewModel>();
                viewModel.RecentBorrowings = new List<RecentTransactionViewModel>();
            }

            return View(viewModel);
        }
    }
}