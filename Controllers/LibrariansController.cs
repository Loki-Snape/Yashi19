using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    public class LibrariansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LibrariansController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchQuery, int pageNumber = 1, int pageSize = 5)
        {
            var query = _context.Librarians.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
                query = query.Where(l => (l.FirstName != null && l.FirstName.Contains(searchQuery)) || (l.LastName != null && l.LastName.Contains(searchQuery)) || (l.Email != null && l.Email.Contains(searchQuery)) || (l.EmployeeId != null && l.EmployeeId.Contains(searchQuery)));

            var totalItems = await query.CountAsync();
            var librarians = await query
                .OrderBy(l => l.LastName)
                .ThenBy(l => l.FirstName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new LibrarianIndexViewModel
            {
                Librarians = librarians,
                SearchQuery = searchQuery,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }
    }
}
