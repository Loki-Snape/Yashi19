using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.ViewModels;

namespace LibraryManagementSystem.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Books
        public async Task<IActionResult> Index(string? searchQuery, int pageNumber = 1)
        {
            var pageSize = 5;
            var query = _context.Books.AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var term = searchQuery.Trim().ToLower();
                query = query.Where(b => 
                    b.Title.ToLower().Contains(term) || 
                    b.Author.ToLower().Contains(term) || 
                    b.ISBN.ToLower().Contains(term)
                );
            }

            var totalItems = await query.CountAsync();
            var books = await query
                .OrderBy(b => b.Title)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var viewModel = new BookListViewModel
            {
                Books = books,
                SearchQuery = searchQuery,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };

            return View(viewModel);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.BorrowRecords)
                .ThenInclude(br => br.Student)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (book == null) return NotFound();

            return View(book);
        }

        // GET: Books/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Author,ISBN,Publisher,PublishDate,TotalCopies")] Book book)
        {
            if (ModelState.IsValid)
            {
                book.AvailableCopies = book.TotalCopies;
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,ISBN,Publisher,PublishDate,TotalCopies,AvailableCopies")] Book book)
        {
            if (id != book.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null) return NotFound();

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Books/Borrow/5
        public async Task<IActionResult> Borrow(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            if (book.AvailableCopies <= 0)
            {
                TempData["ErrorMessage"] = "No copies of this book are currently available for borrowing.";
                return RedirectToAction(nameof(Index));
            }

            ViewData["StudentId"] = new SelectList(await _context.Students.Where(s => s.IsActive).ToListAsync(), "Id", "FullName");
            
            var borrowRecord = new BorrowRecord
            {
                BookId = book.Id,
                Book = book
            };

            return View(borrowRecord);
        }

        // POST: Books/Borrow
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow([Bind("BookId,StudentId,DueDate")] BorrowRecord record)
        {
            var book = await _context.Books.FindAsync(record.BookId);
            if (book == null) return NotFound();

            if (book.AvailableCopies <= 0)
            {
                ModelState.AddModelError("", "No copies available.");
            }

            // Remove Book/Student navigation properties validation since we populate them via EF
            ModelState.Remove("Book");
            ModelState.Remove("Student");

            if (ModelState.IsValid)
            {
                book.AvailableCopies--;
                record.BorrowDate = DateTime.Today;
                record.IsReturned = false;

                _context.Add(record);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["StudentId"] = new SelectList(await _context.Students.Where(s => s.IsActive).ToListAsync(), "Id", "FullName", record.StudentId);
            record.Book = book;
            return View(record);
        }

        // POST: Books/Return/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(int id)
        {
            var record = await _context.BorrowRecords
                .Include(r => r.Book)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (record == null) return NotFound();

            if (!record.IsReturned)
            {
                record.IsReturned = true;
                record.ReturnDate = DateTime.Today;

                if (record.Book != null)
                {
                    record.Book.AvailableCopies++;
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Book returned successfully.";
            }

            return RedirectToAction(nameof(Details), new { id = record.BookId });
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}
