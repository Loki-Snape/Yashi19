using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string ISBN { get; set; } = string.Empty;

        [StringLength(100)]
        public string Publisher { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [Display(Name = "Publish Date")]
        public DateTime PublishDate { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Total copies must be between 0 and 1000.")]
        [Display(Name = "Total Copies")]
        public int TotalCopies { get; set; }

        [Required]
        [Range(0, 1000, ErrorMessage = "Available copies must be between 0 and 1000.")]
        [Display(Name = "Available Copies")]
        public int AvailableCopies { get; set; }

        // Navigation property for borrow records
        public virtual ICollection<BorrowRecord> BorrowRecords { get; set; } = new List<BorrowRecord>();
    }
}
