using System;
using System.Collections.Generic;

namespace LibraryManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalPublications { get; set; }
        public int TotalStudents { get; set; }
        public int TotalLibrarians { get; set; }

        // Borrowing Metrics
        public int ActiveBorrowings { get; set; }
        public int OverdueBorrowings { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalBorrowings { get; set; }

        // Transaction Lists
        public List<RecentTransactionViewModel> RecentTransactions { get; set; } = new();
        public List<RecentTransactionViewModel> RecentBorrowings { get; set; } = new();
    }

    public class RecentTransactionViewModel
    {
        public string BookTitle { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentCardId { get; set; } = string.Empty;
        public int Id { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsReturned { get; set; }
    }
}
