using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.ViewModels
{
    public class PublicationIndexViewModel
    {
        public IEnumerable<Publication> Publications { get; set; } = new List<Publication>();
        public string? SearchQuery { get; set; }
        public string? SearchString { get; set; }
        public PublicationType? SelectedType { get; set; } // Updated from string? to PublicationType?
        public int PageNumber { get; set; } = 1;            // Added for controller/view pagination
        public int PageSize { get; set; } = 10;             // Added for controller/view pagination
        public int PageIndex { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public bool HasPreviousPage => PageNumber > 1 || PageIndex > 1;
        public bool HasNextPage => PageNumber < TotalPages || PageIndex < TotalPages;
    }
}