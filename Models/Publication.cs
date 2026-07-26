using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Models
{
    public class Publication
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(150)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Publication Type")]
        public PublicationType Type { get; set; }

        [StringLength(150)]
        public string Publisher { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Issue Number")]
        public string IssueNumber { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Release Date")]
        public DateTime ReleaseDate { get; set; }

        [StringLength(50)]
        public string Frequency { get; set; } = string.Empty; // e.g. Daily, Weekly, Monthly
    }
}