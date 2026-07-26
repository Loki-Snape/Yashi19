using LibraryManagementSystem.ViewModels;
using LibraryManagementSystem.Models;

namespace LibraryManagementSystem.ViewModels
{
    public class DashboardModel
    {
        public int TotalBooks { get; set; }
        public int TotalStudents { get; set; }
        public int TotalLibrarians { get; set; }
        public int TotalPublications { get; set; }
        public int TotalBorrowings { get; set; }
        public int ActiveBorrowings { get; set; }
        public int OverdueBorrowings { get; set; }
        public List<BorrowRecord> RecentBorrowings { get; set; } = new List<BorrowRecord>();
    }
}
