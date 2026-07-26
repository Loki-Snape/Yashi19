using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class BorrowRecord
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Book")]
        public int BookId { get; set; }
        public virtual Book? Book { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int StudentId { get; set; }
        public virtual StudentModel? Student { get; set; }

        [Required]
        [Display(Name = "Borrow Date")]
        [DataType(DataType.Date)]
        public DateTime BorrowDate { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14); // 2 weeks default

        [Display(Name = "Return Date")]
        [DataType(DataType.Date)]
        public DateTime? ReturnDate { get; set; }

        [Display(Name = "Returned?")]
        public bool IsReturned { get; set; } = false;
    }
}
