using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.ViewModels
{
    public class StudentIndexViewModel
    {
        public IEnumerable<StudentModel> Students { get; set; } = new List<StudentModel>();
        
        public string? SearchQuery { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 5;
        public int TotalItems { get; set; }
        
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }
}
