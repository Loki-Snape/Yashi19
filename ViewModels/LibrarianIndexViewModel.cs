using System;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.ViewModels
{
    public class LibrarianIndexViewModel
    {
        public IEnumerable<LibrarianModel> Librarians { get; set; } = new List<LibrarianModel>();

        public string? SearchQuery { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalItems { get; set; }

        // Fixed syntax – proper C# expression-bodied properties
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
